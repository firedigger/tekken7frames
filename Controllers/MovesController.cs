using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace tekken_frames.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovesController : ControllerBase
    {
        private string TitleCase(string s)
        {
            var lower = s.ToLower();
            return s[0].ToString().ToUpper() + lower.Substring(1);
        }

        private IEnumerable<MoveExcelRow> ReadExcel()
        {
            using (var excel = new ExcelPackage(new FileInfo("wwwroot/TEKKEN 7 FRAME DATA.xlsx")))
            {
                var moves = new List<MoveExcelRow>();
                foreach (var sheet in excel.Workbook.Worksheets)
                {
                    if (sheet.Name == "Sheet115" || sheet.Name == "Legend")
                        continue;
                    var character = TitleCase(sheet.Name);
                    for (var i = 2; i <= sheet.Dimension.Rows; ++i)
                    {
                        string GetValue(int column)
                        {
                            return sheet.Cells[i, column]?.Value?.ToString();
                        }
                        if (sheet.Cells[i, 3].Value == null)
                        {
                            //informational row
                            continue;
                        }
                        moves.Add(new MoveExcelRow
                        {
                            Character = character,
                            Command = GetValue(1),
                            HitLevel = GetValue(2),
                            Damage = GetValue(3),
                            StartUpFrame = GetValue(4),
                            BlockFrame = GetValue(5),
                            HitFrame = GetValue(6),
                            CounterHitFrame = GetValue(7),
                            Notes = GetValue(8)
                        });
                    }
                }
                return moves;
            }
        }

        private FramesAdvantage ParseFramesAdvantage(string frames)
        {
            if (string.IsNullOrWhiteSpace(frames))
                return null;
            frames = frames.ToLower();
            if (frames.Contains("knd"))
                return new FramesAdvantage
                {
                    AdvantageType = AdvantageType.Knockdown
                };
            if (frames.Contains("launch"))
                return new FramesAdvantage
                {
                    AdvantageType = AdvantageType.Launch
                };
            if (frames[0] == '+' || frames[0] == '+' || char.IsDigit(frames[0]))
            {
                var i = 1;
                while (i < frames.Length && char.IsDigit(frames[i]))
                {
                    ++i;
                }
                return new FramesAdvantage
                {
                    AdvantageType = AdvantageType.Normal,
                    Frames = int.Parse(frames.Substring(0, i))
                };
            }
            return null;
        }

        private int ParseFrames(string frames)
        {
            frames = string.Concat(frames.Split('~')[0].TakeWhile(f => f == '+' || f == '-' || char.IsDigit(f)));
            if (string.IsNullOrEmpty(frames))
                return 0;
            return int.Parse(frames);
        }

        private int ParseDamage(string damage)
        {
            if (string.IsNullOrWhiteSpace(damage))
                return 0;
            return damage.Split(',').Select(int.Parse).Sum();
        }

        private string RemoveBetween(string s, char begin, char end)
        {
            return new Regex(string.Format("\\{0}.*?\\{1}", begin, end)).Replace(s, string.Empty);
        }

        private List<HitLevel> ParseHitLevels(string hitLevels)
        {
            hitLevels = RemoveBetween(hitLevels, '(', ')').Replace(",", "").Replace(" ", "");
            var result = new List<HitLevel>();
            for (var i = 0; i < hitLevels.Length; ++i)
            {
                var h = hitLevels[i];
                switch (h)
                {
                    case 'h': result.Add(HitLevel.High); break;
                    case 'm': result.Add(HitLevel.Mid); break;
                    case 'l': result.Add(HitLevel.Low); break;
                    case '!': result.Add(HitLevel.Special); break;
                    case 'S':
                        if (i < hitLevels.Length - 1 && hitLevels[i + 1] == 'm')
                        {
                            result.Add(HitLevel.SpecialMid);
                            ++i;
                        }
                        break;
                }
            }
            return result;
        }

        private Character ParseCharacter(string character)
        {
            return (Character)Enum.Parse(typeof(Character), character.Replace(" ", "").Replace("-", ""), true);
        }

        private MoveProperties ParseMovePropertiesFromNotes(string notes)
        {
            var props = new MoveProperties();
            if (string.IsNullOrWhiteSpace(notes))
                return props;
            if (notes.ToLower().Contains("power crush"))
            {
                props.PowerCrush = true;
            }
            if (notes.ToLower().Contains("Homing"))
            {
                props.Homing = true;
            }
            if (notes.ToLower().Contains("tail spin"))
            {
                props.TailSpin = true;
            }
            if (notes.ToLower().Contains("wall splat"))
            {
                props.WallSplat = true;
            }
            if (notes.ToLower().Contains("wall bounce"))
            {
                props.WallBounce = true;
            }
            if (notes.ToLower().Contains("rage"))
            {
                props.Rage = true;
            }
            return props;
        }

        private Move ParseMove(MoveExcelRow excelMove)
        {
            var move = new Move
            {
                Character = ParseCharacter(excelMove.Character),
                Damage = ParseDamage(excelMove.Damage),
                StartUpFrame = ParseFrames(excelMove.StartUpFrame),
                BlockFrame = ParseFramesAdvantage(excelMove.BlockFrame),
                HitFrame = ParseFramesAdvantage(excelMove.HitFrame),
                CounterHitFrame = ParseFramesAdvantage(excelMove.CounterHitFrame),
                HitLevels = ParseHitLevels(excelMove.HitLevel),
                Notes = excelMove.Notes,
                MoveProperties = ParseMovePropertiesFromNotes(excelMove.Notes)
            };
            var inputParser = new InputParser(excelMove.Command);
            inputParser.Parse();
            move.Stance = inputParser.GetStance();
            move.Commands = inputParser.GetCommands();
            move.MoveProperties.Rage = inputParser.Rage();
            //TODO if "or" then multiply moves
            return move;
        }

        private IEnumerable<Move> GenerateMoves(IEnumerable<MoveExcelRow> excelMoves)
        {
            return excelMoves.Select<MoveExcelRow, Move>(ParseMove);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Move>> Get()
        {
            var excelMoves = ReadExcel();
            var moves = GenerateMoves(excelMoves).Take(10);
            return Ok(moves);
        }
    }
}