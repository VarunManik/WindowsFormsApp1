using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using WindowsFormsApp1.Model;
using Application = System.Windows.Forms.Application;

namespace WindowsFormsApp
{
    public class NameDetailsApp : Form
    {
        private ComboBox searchBox;
        private Button submitButton;
        private Label nameLabel;
        PresentationForm PresentationForm = null;
        public NameDetailsApp()
        {
            AuctionConfig.ReadConfig();


            nameLabel = new Label
            {
                Text = "Enter Name",
                AutoSize = true,
                Location = new System.Drawing.Point(50, 35),
                Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold)
            };

            searchBox = new ComboBox
            {
                Location = new System.Drawing.Point(50, 50),
                Width = 200,
                Height = 70,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.CustomSource,
                Font = new Font("Arial", 12)
            };

            var autoComplete = new AutoCompleteStringCollection();

            autoComplete.AddRange(AuctionConfig.config.FullPlayerList.Select(x => x.Name).ToArray());

            searchBox.AutoCompleteCustomSource = autoComplete;

            Text = "Start APplication";
            Size = new System.Drawing.Size(400, 200);
            submitButton = new Button { Text = "Submit", Width = 100, Height = 40 };

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20),
            };

            flowPanel.Controls.Add(nameLabel);
            flowPanel.Controls.Add(searchBox);
            flowPanel.Controls.Add(submitButton);

            searchBox.Margin = new Padding(10);
            submitButton.Margin = new Padding(10);

            submitButton.Click += OnSubmitButtonClick;

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Name = "Main",
            };
            mainPanel.Controls.Add(flowPanel);
            flowPanel.Anchor = AnchorStyles.None;
            mainPanel.Resize += (sender, e) =>
            {
                flowPanel.Left = (mainPanel.ClientSize.Width - flowPanel.Width) / 2;
                flowPanel.Top = (mainPanel.ClientSize.Height - flowPanel.Height) / 2;
            };

            Controls.Add(mainPanel);

            Text = "Name Details App";
            WindowState = FormWindowState.Maximized;
        }

        private void OnSubmitButtonClick(object sender, EventArgs e)
        {
            string name = searchBox.Text;


            if (!string.IsNullOrWhiteSpace(name) && AuctionConfig.config.PlayerList.ContainsKey(name.ToLower()))
            {
              

                if(PresentationForm != null)
                {
                    PresentationForm.RefreshName(name);
                }
                else
                { 
                    PresentationForm = new PresentationForm(name, this);
                }
                PresentationForm.Show();
                var detailsForm = new DetailsForm(name, PresentationForm, this);
                detailsForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Please enter a valid name. or Name not in the List", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NameDetailsApp());
        }
    }

    public class PresentationForm : Form
    {
        private Form mainForm;
        public TextBox TextBox;
        public Label detailsLabel;
        public PictureBox pictureBox;


        public PresentationForm(string name, Form mainForm)
        {
            this.mainForm = mainForm;

            TextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 600,
                Height = 800,
                Font = new System.Drawing.Font("Arial", 24),
                Location = new System.Drawing.Point(200,40)
            };
            string value = $" Name : {name.ToUpper()} \n Category : {AuctionConfig.config.PlayerList[name.ToLower()].Item2} \n" +
            $" Role : {AuctionConfig.config.PlayerList[name.ToLower()].Item1}";


            detailsLabel = new Label
            {
                Text = value,
                AutoSize = true,
                Location = new System.Drawing.Point(800, 20),
                Font = new System.Drawing.Font("Arial", 24, FontStyle.Bold)
            };

            pictureBox = new PictureBox
            {
                ImageLocation = $"{AuctionConfig.config.PlayerList[name.ToLower()].Item3}",
                Height = 500,
                Width = 600,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new System.Drawing.Point(800, 200)
            };



            Controls.Add(TextBox);
            Controls.Add(detailsLabel);
            Controls.Add(pictureBox);

            Text = "Present Details";
            WindowState = FormWindowState.Maximized ;

            BackColor = Color.White;
        }

        public void RefreshName(string name)
        {

            TextBox.Text = string.Empty;
            string value = $" Name : {name.ToUpper()} \n Category : {AuctionConfig.config.PlayerList[name.ToLower()].Item2} \n" +
            $" Role : {AuctionConfig.config.PlayerList[name.ToLower()].Item1}";

            detailsLabel.Text = value;
            pictureBox.ImageLocation = AuctionConfig.config.PlayerList[name.ToLower()].Item3;

        }
    }


    public class DetailsForm : Form
    {
        private Form mainForm;
        private Label detailsLabel;
        private Button nextButton;
        private Button BackButton;
        private Button UndoButton;
        private TextBox historyTextBox;
        private TextBox FinalValueLabel;
        private Stack<(Button TeamName, long Value)> historyStack = new Stack<(Button, long)>();
        private ComboBox TeamBox;
        private ListView listBox;
        PresentationForm ppform;

        private List<Label> listsOfButtons = new List<Label>();
        private List<ToolTip> listsOfHover = new List<ToolTip>();

        long CurrentplayerValue = 0;
        long Increment = 0;
        int BidCount = 0;

        PlayerInfo playerInfo = null;
        string playerName = string.Empty;
        public DetailsForm(string name, PresentationForm PresentationForm, Form mainForm)
        {
            ppform = PresentationForm;
            this.mainForm = mainForm;

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight
            };

            playerInfo = new PlayerInfo();
            playerInfo.PlayerCategory = AuctionConfig.config.PlayerList[name.ToLower()].Item2;
            playerName = name;


            CurrentplayerValue = AuctionConfig.config.CategoryModel[playerInfo.PlayerCategory].Base;
            Increment = AuctionConfig.config.CategoryModel[playerInfo.PlayerCategory].DefaultIncrement;


            string value = $" Name : {playerName.ToUpper()} \n Category : {playerInfo.PlayerCategory} \n" +
                $" Total Purse Value : {AuctionConfig.config.TotalPurseValue}";


            detailsLabel = new Label
            {
                Text = value,
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 24, FontStyle.Bold)
            };

            BackButton = new Button()
            {
                Text = "Back",
                Width = 100,
                Height = 40
            };

            nextButton = new Button()
            {
                Text = "Sold",
                Width = 100,
                Height = 40
            };

            UndoButton = new Button()
            {
                Text = "Undo",
                Width = 100,
                Height = 40
            };


            var BasePriceLabel = new Label { Text = "Base Price", Font = new System.Drawing.Font("Arial", 12), AutoSize = true };
            var IncrementLabel = new Label { Text = "Increment", Font = new System.Drawing.Font("Arial", 12), AutoSize = true };

            var BaseTextBox = new TextBox { Text = $"{CurrentplayerValue}", Width = 150, Height = 10, Font = new System.Drawing.Font("Arial", 12) };
            var Increment2TextBox = new TextBox { Text = $"{Increment}", Width = 150, Height = 10, Font = new System.Drawing.Font("Arial", 12) };

            BaseTextBox.TextChanged += OnBaseTextChanged;
            Increment2TextBox.TextChanged += OnIncrementTextChanged;

            UndoButton.Click += Undo_ButtonOperation;
            nextButton.Click += OnNextButtonClick;
            BackButton.Click += OnBackButtonClick;


            flowPanel.Controls.Add(detailsLabel);

            flowPanel.Controls.Add(BasePriceLabel);
            flowPanel.Controls.Add(BaseTextBox);

            flowPanel.Controls.Add(IncrementLabel);
            flowPanel.Controls.Add(Increment2TextBox);

            FinalValueLabel = new TextBox
            {
                Text = $"{CurrentplayerValue}",
                Width = 300,
                Height = 100,
                Font = new System.Drawing.Font("Arial", 24),
                Location = new System.Drawing.Point(400, 450)
            };


            FinalValueLabel.TextChanged += onFinalValueLabelChanged;

            mainPanel.Controls.Add(FinalValueLabel);

            TeamBox = new ComboBox
            {
                Location = new System.Drawing.Point(950, 50),
                Width = 150
            };

            TeamBox.Items.AddRange(AuctionConfig.config.TeamNames.ToArray());

            TeamBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

            listBox = new ListView
            {
                Location = new System.Drawing.Point(950, 80),
                Width = 500,
                Height = 380,
                View = View.Details,
                Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold),
                FullRowSelect = true,
                GridLines = true
            };

            listBox.Columns.Add("Category", 140, HorizontalAlignment.Center);
            listBox.Columns.Add("Name", 180, HorizontalAlignment.Center);
            listBox.Columns.Add("Price Sold", 180, HorizontalAlignment.Center);

            mainPanel.Controls.Add(TeamBox);
            mainPanel.Controls.Add(listBox);

            flowPanel.Controls.Add(BackButton);
            flowPanel.Controls.Add(nextButton);
            flowPanel.Controls.Add(UndoButton);

            mainPanel.Controls.Add(flowPanel);


            var flowPanel6 = new FlowLayoutPanel
            {
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                Location = new System.Drawing.Point(30, 160)
            };

            historyTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 300,
                Height = 320,
                Font = new System.Drawing.Font("Arial", 12)
            };

            PictureBox pictureBox = new PictureBox
            {
                ImageLocation = $"{AuctionConfig.config.PlayerList[name.ToLower()].Item3}",

                Size = new System.Drawing.Size(600, 500),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new System.Drawing.Point(700, 120),
                Dock = DockStyle.Fill
            };

            flowPanel6.Controls.Add(pictureBox);
            flowPanel6.Controls.Add(historyTextBox);
            mainPanel.Controls.Add(flowPanel6);

            int totalTeams = AuctionConfig.config.TeamNames.Count;



            var flowPanel2 = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var flowPanel4 = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var details = TeamDetails.Instance.Details;

            for (int i = 0; i < totalTeams; i++)
            {

                var flowPanel3 = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FlowDirection = FlowDirection.TopDown
                };

                Button button = new Button
                {
                    Text = AuctionConfig.config.TeamNames[i],
                    Name = AuctionConfig.config.TeamNames[i],
                    Font = new System.Drawing.Font("Arial", 20),
                    Size = new System.Drawing.Size(230, 50),
                    Location = new System.Drawing.Point(200 + ((i) % (totalTeams / 2)) * 180, 300 + ((i) / (totalTeams / 2)) * 80)
                };

                var toolTip = new ToolTip();
                toolTip.SetToolTip(button, $"{TeamDetails.Instance.Details[AuctionConfig.config.TeamNames[i]].MinPlayerReq.ToString()}");


                Label button1Label = new Label
                {
                    Text = $" Purse Utilized : {details[AuctionConfig.config.TeamNames[i]].PurseUtilized} \n Remaining Purse : {details[AuctionConfig.config.TeamNames[i]].PurseRem} \n BaseRequired : {details[AuctionConfig.config.TeamNames[i]].MinBaseRequired} \n ExcessBid : {details[AuctionConfig.config.TeamNames[i]].PurseRem - (details[AuctionConfig.config.TeamNames[i]].MinBaseRequired - AuctionConfig.config.CategoryModel[playerInfo.PlayerCategory].Base)} \n",
                    Name = AuctionConfig.config.TeamNames[i],
                    Font = new System.Drawing.Font("Arial", 12),
                    AutoSize = true,
                    Size = new System.Drawing.Size(230, 50),
                    Location = new System.Drawing.Point(200 + ((i) % (totalTeams / 2)) * 180, 300 + ((i) / (totalTeams / 2)) * 80)
                };

                listsOfButtons.Add(button1Label);

                // Add a Click event handler
                button.Click += Button_Click;
                // Add the button to the form
                flowPanel3.Controls.Add(button);
                flowPanel3.Controls.Add(button1Label);
                if (i < totalTeams / 2)
                {
                    flowPanel2.Controls.Add(flowPanel3);
                }
                else
                {
                    flowPanel4.Controls.Add(flowPanel3);
                }

            }
            mainPanel.Controls.Add(flowPanel2);
            mainPanel.Controls.Add(flowPanel4);

            Controls.Add(mainPanel);

            // Set form properties
            Text = "Name Details";
            WindowState = FormWindowState.Maximized;
        }


        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox.Items.Clear();

            string tname = TeamBox.SelectedItem.ToString();

            List<(CATEGORIES, string, string)> tuples = TeamDetails.Instance.Details[tname].playerInfos.Select(kv => (kv.Value.PlayerCategory, kv.Key, kv.Value.PriceSold.ToString())).OrderBy(z => z.Item1).ToList();


            foreach (var item in tuples)
            {
                var listViewItem = new ListViewItem(new[] { item.Item1.ToString(), item.Item2, item.Item3 });
                listBox.Items.Add(listViewItem);
            }
        }

        private void onFinalValueLabelChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            long.TryParse(textBox.Text, out long res);
            CurrentplayerValue = res;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button CurrentButton = sender as Button;
            Label Currentlabel = listsOfButtons.Where(x => x.Name == CurrentButton.Name).First();

            if (!EligiblityToBid(CurrentButton.Name, out string reason))
            {
                MessageBox.Show($"Failed due to - {reason}", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BidCount++;

            if (historyStack.Count > 0)
            {
                var preButton = historyStack.Peek().TeamName;
                preButton.BackColor = Color.White;
            }

            historyStack.Push((CurrentButton, CurrentplayerValue));

            CurrentplayerValue += Increment;

            FinalValueLabel.Text = CurrentplayerValue.ToString();
            CurrentButton.BackColor = Color.LightGreen;

            UpdateHistoryDisplay();
        }

        private bool EligiblityToBid(string tName, out string reason)
        {
            reason = string.Empty;


            var details = TeamDetails.Instance.Details[tName];

            if (historyStack.Count() != 0)
            {
                if (historyStack.Peek().TeamName.Name == tName)
                {
                    reason = "Cannot bid same team Twice !!!";
                    return false;
                }
            }

            if (details.MinPlayerReq.reqCount <= 0)
            {
                reason = "Squad Already Completed !!!";
                return false;
            }

            if (details.MinPlayerReq.diCategoryWiseCount[playerInfo.PlayerCategory] <= 0)
            {
                reason = "Category Count Already Satisifed !!!";
                return false;
            }

            var value = details.PurseRem - (details.MinBaseRequired - AuctionConfig.config.CategoryModel[playerInfo.PlayerCategory].Base);

            var topup = details.PurseUtilized + details.MinBaseRequired - AuctionConfig.config.TotalPurseValue;

            if (CurrentplayerValue > value)
            {
                bool forced = WantToTakeTopup(topup);
                
                if(forced == true)
                {
                    if(value <= -400000)
                    {
                        reason = "TOP up LIMIT exceeded";
                        return false;
                    }
                    return true;
                }

                reason = "Low on Budget !!!";
                return false;
            }

            return true;
        }

        private bool WantToTakeTopup(long topup)
        {
            var result = MessageBox.Show($"Do you want to take topup? {topup}", "Confirmation", MessageBoxButtons.OKCancel);

            if (DialogResult.OK == result)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnBackButtonClick(object sender, EventArgs e)
        {
            this.Close();
            mainForm.Show();
        }

        private void OnIncrementTextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            long.TryParse(textBox.Text, out long res);

            Increment = res;
            return;
        }

        private void OnBaseTextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            long.TryParse(textBox.Text, out long res);

            CurrentplayerValue = res;
            return;
        }

        private void Undo_ButtonOperation(object sender, EventArgs e)
        {
            BidCount--;
            if (historyStack.Count > 0)
            {
                var lastAction = historyStack.Pop();

                var button = lastAction.TeamName as Button;
                button.BackColor = Color.White;

                if (historyStack.Count > 0)
                {
                    var preButton = historyStack.Peek().TeamName;
                    preButton.BackColor = Color.LightGreen;
                }

                UpdateHistoryDisplay();
                CurrentplayerValue = Math.Max(CurrentplayerValue - Increment, AuctionConfig.config.CategoryModel[playerInfo.PlayerCategory].Base);
                FinalValueLabel.Text = CurrentplayerValue.ToString();
            }
            else
            {
                MessageBox.Show("No actions to undo.", "Undo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateHistoryDisplay()
        {
            var historyText = new List<string>();

            foreach (var action in historyStack)
            {
                historyText.Add($"{action.TeamName.Name} Bids {action.Value} ");
            }

            var val = string.Join(Environment.NewLine, historyText);

            historyTextBox.Text = val;
            ppform.TextBox.Text = val;
            historyTextBox.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);

        }


        private void OnNextButtonClick(object sender, EventArgs e)
        {
            if (historyStack.Count == 0)
            {
                MessageBox.Show($"Player is Unsold - {playerName}", "Unsold", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string tName = historyStack.Peek().TeamName.Name;
                DialogResult result = MessageBox.Show($" Player - {playerName} \n Sold at - {CurrentplayerValue - Increment}\n To - {tName}  ", "Confirmation", MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                {
                    playerInfo.PriceSold = CurrentplayerValue - Increment;
                    TeamDetails.Instance.Details[tName].playerInfos[playerName] = playerInfo;
                    TeamDetails.Instance.Details[tName].PurseRem -= playerInfo.PriceSold;
                    TeamDetails.Instance.Details[tName].PurseUtilized += playerInfo.PriceSold;
                    TeamDetails.Instance.Details[tName].MinPlayerReq.diCategoryWiseCount[playerInfo.PlayerCategory]--;
                    TeamDetails.Instance.Details[tName].MinPlayerReq.reqCount--;
                    TeamDetails.Instance.Details[tName].MinBaseRequired = TeamDetails.GetBaseRequired(TeamDetails.Instance.Details[tName].MinPlayerReq.diCategoryWiseCount);
                    TeamDetails.SaveInCache();
                }
                else
                {
                    return;
                }
            }
            this.Close();
            this.Dispose();
            mainForm.Show();
        }
    }
}
