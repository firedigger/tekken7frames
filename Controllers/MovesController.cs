using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            return 0;
        }

        private int ParseDamage(string damage)
        {
            //int.Parse(excelMove.Damage)
            return 0;
        }

        private HitLevel ParseHitLevel(string hitLevel)
        {
            return HitLevel.High;
        }

        private Command ParseCommand(string Command)
        {
            return new Command
            {
                ButtonInput = new ButtonInput(),
                DirectionalInput = DirectionalInput.db
            };
        }

        private SingleMove ParseSingleMove(MoveExcelRow excelMove)
        {
            return new SingleMove
            {
                Character = (Character)Enum.Parse(typeof(Character), excelMove.Character),
                Damage = ParseDamage(excelMove.Damage),
                StartUpFrame = ParseFrames(excelMove.StartUpFrame),
                BlockFrame = ParseFramesAdvantage(excelMove.BlockFrame),
                HitFrame = ParseFramesAdvantage(excelMove.HitFrame),
                CounterHitFrame = ParseFramesAdvantage(excelMove.CounterHitFrame),
                HitLevel = ParseHitLevel(excelMove.HitLevel),
                Notes = excelMove.Notes
            };
        }

        private StringMove ParseStringMove(MoveExcelRow excelMove)
        {
            var stringMove = new StringMove
            {
                Character = (Character)Enum.Parse(typeof(Character), excelMove.Character),
                Damage = excelMove.Damage.Split(",").Select(d => int.Parse(d)).Sum(),
                BlockFrame = ParseFramesAdvantage(excelMove.BlockFrame),
                CounterHitFrame = ParseFramesAdvantage(excelMove.CounterHitFrame),
                HitFrame = ParseFramesAdvantage(excelMove.HitFrame),
                StartUpFrame = ParseFrames(excelMove.StartUpFrame),
                MoveProperties = new MoveProperties(),
                Commands = null,
                HitLevels = null,
                Notes = null
            };
            return stringMove;
        }

        private IEnumerable<Move> GenerateMoves(IEnumerable<MoveExcelRow> excelMoves)
        {
            return excelMoves.Where(e => e.Character == "Josie").Select<MoveExcelRow, Move>(excelMove =>
            {
                if (excelMove.Command.Contains(","))
                {
                    return ParseStringMove(excelMove);
                }

                return ParseSingleMove(excelMove);
            });
        }

        [HttpGet]
        public ActionResult<IEnumerable<Move>> Get()
        {
            var excelMoves = ReadExcel();
            var moves = GenerateMoves(excelMoves);
            return Ok(moves);
        }
    }
}