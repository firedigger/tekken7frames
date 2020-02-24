using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            return new FramesAdvantage();
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
            //int.Parse(excelMove.Damage)
            return 0;
        }

        private List<HitLevel> ParseHitLevels(string hitLevel)
        {
            return new List<HitLevel> { HitLevel.High };
        }

        private Character ParseCharacter(string character)
        {
            return (Character)Enum.Parse(typeof(Character), character.Replace(" ", "").Replace("-", ""), true);
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
                MoveProperties = new MoveProperties()
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