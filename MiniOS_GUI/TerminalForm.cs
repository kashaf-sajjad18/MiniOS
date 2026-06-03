using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class TerminalForm : Form
    {
        private RichTextBox terminalBox;
        private bool booted = false;
        private bool calcMode = false;
        private double calcNum1 = 0;
        private string calcOp = "";
        private bool calculatorRunning = false;

        public TerminalForm()
        {
            BuildUI();
            this.Shown += OnShown;
        }

        private void BuildUI()
        {
            this.Text = "MiniOS Terminal";
            this.Size = new Size(1000, 650);
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.CenterScreen;

            terminalBox = new RichTextBox();
            terminalBox.BackColor = Color.Black;
            terminalBox.ForeColor = Color.Lime;
            terminalBox.Font = new Font("Consolas", 11, FontStyle.Bold);
            terminalBox.Dock = DockStyle.Fill;
            terminalBox.WordWrap = false;
            terminalBox.KeyDown += TerminalBox_KeyDown;

            Controls.Add(terminalBox);
        }

        private void OnShown(object sender, EventArgs e)
        {
            BootSystem();
            terminalBox.Focus();
        }

        private void TerminalBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RunCommand();
            }
        }

        private void RunCommand()
        {
            string fullText = terminalBox.Text;
            int promptPos = fullText.LastIndexOf("MiniOS> ");

            if (promptPos >= 0)
            {
                string command = fullText.Substring(promptPos + 8);
                command = command.Trim();
                terminalBox.AppendText("\n");
                ProcessCommand(command);
                if (!calcMode) terminalBox.AppendText("MiniOS> ");
                terminalBox.ScrollToCaret();
            }
        }

        private void BootSystem()
        {
            string[] bootMessages = {
                "Initializing Kernel............. [OK]",
                "Loading Memory Manager.......... [OK]",
                "Starting Task Service........... [OK]",
                "Mounting File System............ [OK]",
                "Loading Shell................... [OK]"
            };

            foreach (string msg in bootMessages)
            {
                terminalBox.AppendText(msg + "\n");
                terminalBox.ScrollToCaret();
                Thread.Sleep(400);
                Application.DoEvents();
            }

            Thread.Sleep(300);
            terminalBox.AppendText("\n========== MINIOS READY ==========\n\n");
            Thread.Sleep(200);

            ShowWelcome();
            booted = true;
            terminalBox.AppendText("MiniOS> ");
            terminalBox.ScrollToCaret();
        }

        private void ShowWelcome()
        {
            terminalBox.AppendText("========= MINIOS TERMINAL =========\n");
            terminalBox.AppendText("Available Commands:\n");
            terminalBox.AppendText("help - show commands\n");
            terminalBox.AppendText("time - show time\n");
            terminalBox.AppendText("calc - calculator\n");
            terminalBox.AppendText("mem  - memory status\n");
            terminalBox.AppendText("task - task manager\n");
            terminalBox.AppendText("shutdown - exit\n\n");
        }

        private void ProcessCommand(string input)
        {
            if (!booted) return;
            if (calcMode)
            {
                HandleCalc(input);
                return;
            }
            if (string.IsNullOrEmpty(input)) return;

            string cmd = input.ToLower();
            switch (cmd)
            {
                case "help": ShowWelcome(); break;
                case "time": terminalBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + "\n"); break;
                case "calc": EnterCalcMode(); break;
                case "mem": ShowMem(); break;
                case "task": OpenTaskManager(); break;
                case "shutdown": terminalBox.AppendText("System Shutting Down...\n"); Thread.Sleep(800); this.Close(); break;
                default: terminalBox.AppendText("Unknown Command!\n"); break;
            }
        }

        private void OpenTaskManager()
        {
            foreach (Form form in Application.OpenForms)
                if (form is TaskManagerForm) { form.BringToFront(); terminalBox.AppendText("\n[Task Manager already open]\n"); return; }
            new TaskManagerForm().Show();
            terminalBox.AppendText("\n[Task Manager opened]\n");
        }

        private void ShowMem()
        {
            foreach (Form form in Application.OpenForms)
                if (form is MemoryForm) { form.BringToFront(); terminalBox.AppendText("\n[Memory Manager already open]\n"); return; }
            new MemoryForm().Show();
            terminalBox.AppendText("\n[Memory Manager opened]\n");
        }

        private void EnterCalcMode()
        {
            calcMode = true;
            calcNum1 = 0;
            calcOp = "";
            terminalBox.AppendText("\n=== CALCULATOR MODE ===\n");
            terminalBox.AppendText("First Number: ");
            terminalBox.ScrollToCaret();
        }

        private void HandleCalc(string input)
        {
            input = input.Trim();

            if (calcNum1 == 0 && string.IsNullOrEmpty(calcOp))
            {
                if (double.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num1))
                {
                    calcNum1 = num1;
                    terminalBox.AppendText("Operator (+ - * /): ");
                }
                else
                {
                    terminalBox.AppendText("Invalid number! Use digits (e.g., 10, 3.5)\n");
                    terminalBox.AppendText("First Number: ");
                }
            }
            else if (string.IsNullOrEmpty(calcOp))
            {
                if (input == "+" || input == "-" || input == "*" || input == "/")
                {
                    calcOp = input;
                    terminalBox.AppendText("Second Number: ");
                }
                else
                {
                    terminalBox.AppendText("Invalid operator! Use + - * /\n");
                    terminalBox.AppendText("Operator (+ - * /): ");
                }
            }
            else
            {
                if (double.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num2))
                {
                    double result = 0;
                    bool error = false;
                    switch (calcOp)
                    {
                        case "+": result = calcNum1 + num2; break;
                        case "-": result = calcNum1 - num2; break;
                        case "*": result = calcNum1 * num2; break;
                        case "/":
                            if (num2 == 0) { terminalBox.AppendText("Error: Division by zero!\n"); error = true; }
                            else result = calcNum1 / num2;
                            break;
                    }
                    if (!error) terminalBox.AppendText($"\nResult = {Math.Round(result, 2):F2}\n");
                    if (!calculatorRunning) { calculatorRunning = true; UpdateMemoryForm(); }
                }
                else terminalBox.AppendText("Invalid number! Use digits (e.g., 10, 3.5)\n");

                calcMode = false;
                calcNum1 = 0;
                calcOp = "";
                terminalBox.AppendText("\nMiniOS> ");
            }
            terminalBox.ScrollToCaret();
        }

        private void UpdateMemoryForm()
        {
            foreach (Form form in Application.OpenForms)
                if (form is MemoryForm m) m.SetCalculatorStatus(calculatorRunning);
                else if (form is TaskManagerForm t) { if (calculatorRunning) t.StartCalculator(); else t.StopCalculator(); }
        }

        public void StartCalculator() { calculatorRunning = true; UpdateMemoryForm(); }
        public void StopCalculator() { calculatorRunning = false; UpdateMemoryForm(); }
    }
}