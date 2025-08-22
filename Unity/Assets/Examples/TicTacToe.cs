#nullable enable

using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Internal;
using NeuroSdk.Json;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NeuroSdk.Examples
{
    public class TicTacToe : MonoBehaviour
    {
        [SerializeField]
        private GameObject resetButton = null!;

        private bool _playerTurn = true;

        private void Start()
        {
            Context.Send("A Tic Tac Toe game has started. You are playing as O.", true);
        }

        public void PlayerPlayInCell(GameObject cell)
        {
            if (!_playerTurn) return;
            if (cell.transform.GetChild(0).gameObject.activeSelf || cell.transform.GetChild(1).gameObject.activeSelf) return;

            _playerTurn = false;

            cell.transform.GetChild(0).gameObject.SetActive(true);
            Context.Send($"Your opponent played an X in the {cell.name} cell.", false);

            if (!CheckWin())
            {
                ActionWindow.Create(gameObject)
                    .SetForce(0, "It is your turn. Please place an O.", "", false)
                    .AddAction(new PlayOAction(this))
                    .Register();
            }
            else
            {
                EnableReset();
            }
        }

        public void BotPlayInCell(GameObject cell)
        {
            cell.transform.GetChild(1).gameObject.SetActive(true);
            if (!CheckWin())
            {
                _playerTurn = true;
            }
            else
            {
                EnableReset();
            }
        }

        private bool CheckWin()
        {
            if (CheckLine(0, 0, 1, 2) || CheckLine(0, 3, 4, 5) || CheckLine(0, 6, 7, 8) ||
                CheckLine(0, 0, 3, 6) || CheckLine(0, 1, 4, 7) || CheckLine(0, 2, 5, 8) ||
                CheckLine(0, 0, 4, 8) || CheckLine(0, 2, 4, 6))
            {
                Context.Send("You lost. Better luck next time.", false);
                return true;
            }

            if (CheckLine(1, 0, 1, 2) || CheckLine(1, 3, 4, 5) || CheckLine(1, 6, 7, 8) ||
                CheckLine(1, 0, 3, 6) || CheckLine(1, 1, 4, 7) || CheckLine(1, 2, 5, 8) ||
                CheckLine(1, 0, 4, 8) || CheckLine(1, 2, 4, 6))
            {
                Context.Send("You won. Congratulations.", false);
                return true;
            }

            if (transform.Cast<Transform>().Take(9).All(c => c.GetChild(0).gameObject.activeSelf || c.GetChild(1).gameObject.activeSelf))
            {
                Context.Send("It's a tie. No one won.", false);
                return true;
            }

            return false;
        }

        private bool CheckLine(int player, int c1, int c2, int c3)
        {
            return transform.GetChild(c1).GetChild(player).gameObject.activeSelf &&
                   transform.GetChild(c2).GetChild(player).gameObject.activeSelf &&
                   transform.GetChild(c3).GetChild(player).gameObject.activeSelf;
        }

        private void EnableReset()
        {
            resetButton.SetActive(true);
        }

        public void ResetBoard()
        {
            resetButton.SetActive(false);

            foreach (Transform cell in transform)
            {
                cell.GetChild(0).gameObject.SetActive(false);
                cell.GetChild(1).gameObject.SetActive(false);
            }

            Context.Send("A new Tic Tac Toe game has started. You are playing as O.", true);

            _playerTurn = true;
        }
    }

    public class PlayOAction : NeuroAction<GameObject>
    {
        private readonly TicTacToe _ticTacToe;

        public PlayOAction(TicTacToe ticTacToe)
        {
            _ticTacToe = ticTacToe;
        }

        public override string Name => "play";
        protected override string Description => "Place an O in the specified cell.";

        protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
            Required = new List<string> { "cell" },
            Properties = new Dictionary<string, JsonSchema>
            {
                ["cell"] = QJS.Enum(GetAvailableCells())
            }
        };

        protected override ExecutionResult Validate(ActionJData actionData, out GameObject? cell)
        {
            string? desiredCell = actionData.Data?["cell"]?.Value<string>();
            if (string.IsNullOrEmpty(desiredCell))
            {
                cell = null;
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedMissingRequiredParameter.Format("cell"));
            }

            string[] cells = GetAvailableCells().ToArray();
            if (!cells.Contains(desiredCell))
            {
                cell = null;
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedInvalidParameter.Format("cell"));
            }

            cell = _ticTacToe.transform.Find(desiredCell)?.gameObject;
            return ExecutionResult.Success();
        }

        protected override void Execute(GameObject? cell)
        {
            _ticTacToe.BotPlayInCell(cell!);
        }

        private IEnumerable<string> GetAvailableCells()
        {
            for (int i = 0; i < 9; i++)
            {
                if (!_ticTacToe.transform.GetChild(i).GetChild(0).gameObject.activeSelf &&
                    !_ticTacToe.transform.GetChild(i).GetChild(1).gameObject.activeSelf)
                {
                    yield return _ticTacToe.transform.GetChild(i).name;
                }
            }
        }
    }
}
