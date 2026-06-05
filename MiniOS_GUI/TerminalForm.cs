using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class TerminalForm : Form
    {
        private RichTextBox terminalBox;
        private bool booted = false;
        private bool commandRunning = false;
        private string currentInput = "";
        private bool calculatorMode = false;

        // Boot timer
        private string[] bootLines;
        private int bootIndex = 0;
        private System.Windows.Forms.Timer bootTimer;

        public TerminalForm()
        {
            BuildUI();
            this.Load += (s, e) => StartBoot();
        }

        // -------------------------------------------------------
        // UI BUILD
        // -------------------------------------------------------
        private void BuildUI()
        {
            this.Text = "NOVA-OS Terminal";
            this.Size = new Size(900, 600);
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Consolas", 11, FontStyle.Bold);

            terminalBox = new RichTextBox();
            terminalBox.BackColor = Color.Black;
            terminalBox.ForeColor = Color.Lime;
            terminalBox.Font = new Font("Consolas", 11, FontStyle.Bold);
            terminalBox.Dock = DockStyle.Fill;
            terminalBox.ReadOnly = false;
            terminalBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            terminalBox.WordWrap = true;
            terminalBox.KeyDown += TerminalBox_KeyDown;
            terminalBox.KeyPress += TerminalBox_KeyPress;
            Controls.Add(terminalBox);
        }

        // -------------------------------------------------------
        // BOOT — Timer based, line by line, non-blocking
        // -------------------------------------------------------
        private void StartBoot()
        {
            bootLines = new string[] {
                "",
                "Initializing Kernel............. [OK]",
                "Loading Memory Manager.......... [OK]",
                "Starting Task Service........... [OK]",
                "Mounting File System............ [OK]",
                "Loading Shell................... [OK]",
                "",
                "========== MINIOS READY ==========",
                ""
            };

            bootTimer = new System.Windows.Forms.Timer();
            bootTimer.Interval = 350;
            bootTimer.Tick += BootTick;
            bootTimer.Start();
        }

        private void BootTick(object sender, EventArgs e)
        {
            if (bootIndex < bootLines.Length)
            {
                terminalBox.AppendText(bootLines[bootIndex] + "\n");
                terminalBox.ScrollToCaret();
                bootIndex++;
            }
            else
            {
                bootTimer.Stop();
                bootTimer.Dispose();

                // ── Only show "Type help" note after boot, NOT the full menu ──
                terminalBox.AppendText("  Type 'help' for available commands.\n\n");

                booted = true;
                Prompt();
                terminalBox.Focus();
            }
        }

        // -------------------------------------------------------
        // KEY HANDLING
        // -------------------------------------------------------
        private void TerminalBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!booted || commandRunning)
            {
                if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down &&
                    e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                    e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (calculatorMode)
                    RunCalculatorExpression();
                else
                    RunCommand();
            }
            else if (e.KeyCode == Keys.Back)
            {
                e.SuppressKeyPress = true;
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    RedrawPrompt();
                }
            }
        }

        private void TerminalBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!booted || commandRunning) { e.Handled = true; return; }

            if (e.KeyChar >= 32 && e.KeyChar != 127)
            {
                currentInput += e.KeyChar;
                RedrawPrompt();
            }
            e.Handled = true;
        }

        private void RedrawPrompt()
        {
            string text = terminalBox.Text;
            int last = text.LastIndexOf('\n');
            if (last < 0) last = 0;
            string before = text.Substring(0, last + 1);
            terminalBox.Text = before + GetPromptText() + currentInput;
            terminalBox.SelectionStart = terminalBox.Text.Length;
            terminalBox.ScrollToCaret();
        }

        // -------------------------------------------------------
        // COMMAND RUNNER
        // -------------------------------------------------------
        private void RunCommand()
        {
            string command = currentInput.Trim();
            currentInput = "";
            terminalBox.AppendText("\n");

            if (string.IsNullOrEmpty(command))
            {
                Prompt();
                return;
            }

            switch (command.ToLower())
            {
                // ── help: show full menu ──
                case "help":
                    ShowHelpMenu();
                    Prompt();
                    break;

                case "clear":
                    terminalBox.Clear();
                    Prompt();
                    break;

                case "note":
                    new BuiltInNotepad(null, true).Show();
                    terminalBox.AppendText("[Notepad opened]\n");
                    Prompt();
                    break;

                case "shutdown":
                    terminalBox.AppendText("Shutting down NOVA-OS...\n");
                    var shutTimer = new System.Windows.Forms.Timer();
                    shutTimer.Interval = 700;
                    shutTimer.Tick += (s, e) => { shutTimer.Stop(); this.Close(); };
                    shutTimer.Start();
                    break;

                // ── time: show time from backend ──
                case "time":
                    string timeOut = MiniOSBackend.RunToString("time");
                    if (!string.IsNullOrEmpty(timeOut))
                        terminalBox.AppendText(timeOut + "\n");
                    else
                        terminalBox.AppendText("Current Time: " + DateTime.Now.ToString("HH:mm:ss") + "\n");
                    Prompt();
                    break;

                // ── mem: ONLY open MemoryForm window, no terminal text, no kernel ──
                case "mem":
                    OpenFormOnce<MemoryForm>();
                    Prompt();
                    break;

                // ── task: ONLY open TaskManagerForm window, no terminal text ──
                case "task":
                    OpenFormOnce<TaskManagerForm>();
                    Prompt();
                    break;

                // ── calc: expression evaluator inline ──
                case "calc":
                    calculatorMode = true;
                    terminalBox.AppendText("Enter expression (e.g. 10+5 or 20/4) then press Enter:\n");
                    terminalBox.AppendText(GetPromptText());
                    terminalBox.ScrollToCaret();
                    commandRunning = false;
                    return;

                case "shell":
                    terminalBox.AppendText("Shell is already running inside this terminal.\n");
                    Prompt();
                    break;

                default:
                    terminalBox.AppendText($"  Unknown command: '{command}'\n");
                    terminalBox.AppendText("  Type 'help' for available commands.\n");
                    Prompt();
                    break;
            }
        }

        // -------------------------------------------------------
        // HELPERS
        // -------------------------------------------------------
        private void Prompt()
        {
            terminalBox.AppendText(GetPromptText());
            terminalBox.ScrollToCaret();
        }

        private string GetPromptText()
        {
            return calculatorMode ? "CALC> " : "NOVA-OS> ";
        }

        // Open a form only once — if already open bring it to front
        private void OpenFormOnce<T>() where T : Form, new()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is T)
                {
                    f.BringToFront();
                    return;
                }
            }
            new T().Show();
        }

        private void RunCalculatorExpression()
        {
            string expression = currentInput.Trim();
            currentInput = "";
            terminalBox.AppendText("\n");

            if (string.IsNullOrEmpty(expression))
            {
                calculatorMode = false;
                Prompt();
                return;
            }

            // Try backend first, fallback to DataTable
            string backendResult = MiniOSBackend.RunToString("calc " + expression);
            if (!string.IsNullOrEmpty(backendResult))
            {
                terminalBox.AppendText("Result: " + backendResult + "\n");
            }
            else
            {
                try
                {
                    DataTable table = new DataTable();
                    object result = table.Compute(expression, string.Empty);
                    terminalBox.AppendText($"Result: {result}\n");
                }
                catch (Exception ex)
                {
                    terminalBox.AppendText($"Error: {ex.Message}\n");
                }
            }

            calculatorMode = false;
            Prompt();
        }

        private void ShowHelpMenu()
        {
            terminalBox.AppendText("\n");
            terminalBox.AppendText("+--------------------------------------------------------------+\n");
            terminalBox.AppendText("|                    AVAILABLE COMMANDS                       |\n");
            terminalBox.AppendText("+--------------------------------------------------------------+\n");
            terminalBox.AppendText("|  help      Show this menu                                   |\n");
            terminalBox.AppendText("|  time      Show current time  (Assembly backend)            |\n");
            terminalBox.AppendText("|  calc      Calculator / expression evaluator                |\n");
            terminalBox.AppendText("|  mem       Open Memory Manager window                       |\n");
            terminalBox.AppendText("|  task      Open Task Manager window                         |\n");
            terminalBox.AppendText("|  note      Built-in notepad                                 |\n");
            terminalBox.AppendText("|  clear     Clear terminal screen                            |\n");
            terminalBox.AppendText("|  shutdown  Shut down NOVA-OS                                |\n");
            terminalBox.AppendText("+--------------------------------------------------------------+\n");
            terminalBox.AppendText("\n");
        }
    }
}