using Adw;
using Gtk;
using Gio;
using System.Runtime.InteropServices;

internal class Program
{
    private static int Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var baseDir = AppContext.BaseDirectory;
            // Tells GTK where the icons and themes are
            Environment.SetEnvironmentVariable("XDG_DATA_DIRS", baseDir);
            // Tells GLib where the settings schemas (gschemas.compiled) are
            Environment.SetEnvironmentVariable("GSETTINGS_SCHEMA_DIR", Path.Combine(baseDir, "share", "glib-2.0", "schemas"));
        }
        var app = Adw.Application.New("com.Belahcensymow.gircoretest", ApplicationFlags.FlagsNone);
        app.OnActivate += (sender, args) =>
        {
            var window = Adw.ApplicationWindow.New((Adw.Application)sender);
            window.Title = "Test";
            window.SetDefaultSize(800, 600);
            var toolbarView = ToolbarView.New();
            var topBar = Adw.HeaderBar.New();
            var statsBox = Box.New(Orientation.Horizontal, 0);
            statsBox.Homogeneous = true;
            statsBox.Hexpand = true;
            statsBox.MarginTop = 10;
            statsBox.MarginBottom = 10;
            toolbarView.AddTopBar(topBar);
            int characters = 0;
            int faults = 0;
            int percentage = 0;
            int time = 0;
            int speed = 0;
            uint timerId = 0;
            bool adjusting = false;
            string[] lines = [""];
            int lineCounter = 0;
            string[] levels = [""];
            int levelCounter = 0;
            bool timeStop = true;

            (string Label, uint KeyVal, int Width)[][] rows =
            [
                [("` ~", Gdk.Constants.KEY_quoteleft, 4), ("1 !", Gdk.Constants.KEY_1, 4), ("2 @", Gdk.Constants.KEY_2, 4), ("3 #", Gdk.Constants.KEY_3, 4), ("4 %", Gdk.Constants.KEY_4, 4), ("5 %", Gdk.Constants.KEY_5, 4), ("6 ^", Gdk.Constants.KEY_6, 4), ("7 &", Gdk.Constants.KEY_7, 4), ("8 *", Gdk.Constants.KEY_8, 4), ("9 (", Gdk.Constants.KEY_9, 4), ("0 )", Gdk.Constants.KEY_0, 4), ("- _", Gdk.Constants.KEY_minus, 4), ("= +", Gdk.Constants.KEY_equal, 4)],
                [("Q", Gdk.Constants.KEY_q, 4), ("W", Gdk.Constants.KEY_w, 4), ("E", Gdk.Constants.KEY_e, 4), ("R", Gdk.Constants.KEY_r, 4), ("T", Gdk.Constants.KEY_t, 4), ("Y", Gdk.Constants.KEY_y, 4), ("U", Gdk.Constants.KEY_u, 4), ("I", Gdk.Constants.KEY_i, 4), ("O", Gdk.Constants.KEY_o, 4), ("P", Gdk.Constants.KEY_p, 4), ("[ {", Gdk.Constants.KEY_bracketleft, 4), ("] }", Gdk.Constants.KEY_bracketright, 4)],
                [("A", Gdk.Constants.KEY_a, 4), ("S", Gdk.Constants.KEY_s, 4), ("D", Gdk.Constants.KEY_d, 4), ("F", Gdk.Constants.KEY_f, 4), ("G", Gdk.Constants.KEY_g, 4), ("H", Gdk.Constants.KEY_h, 4), ("J", Gdk.Constants.KEY_j, 4), ("K", Gdk.Constants.KEY_k, 4), ("L", Gdk.Constants.KEY_l, 4), ("; :", Gdk.Constants.KEY_semicolon, 4), ("' \"", Gdk.Constants.KEY_apostrophe, 4), ("\\ |", Gdk.Constants.KEY_backslash, 4)],
                [("Z", Gdk.Constants.KEY_z, 4), ("X", Gdk.Constants.KEY_x, 4), ("C", Gdk.Constants.KEY_c, 4), ("V", Gdk.Constants.KEY_v, 4), ("B", Gdk.Constants.KEY_b, 4), ("N", Gdk.Constants.KEY_n, 4), ("M", Gdk.Constants.KEY_m, 4), (", <", Gdk.Constants.KEY_comma, 4), (". >", Gdk.Constants.KEY_period, 4), ("/ ?", Gdk.Constants.KEY_slash, 4)],
                [("Shift", Gdk.Constants.KEY_Shift_L, 8), ("Space", Gdk.Constants.KEY_space, 28), ("Shift", Gdk.Constants.KEY_Shift_R, 8)]
            ];
            var grid = new Gtk.Grid
            {
                ColumnSpacing = 6,
                RowSpacing = 6,
                Valign = Align.Fill,
                Halign = Align.Center,
                MarginTop = 20,
                MarginBottom = 20,
                MarginStart = 20,
                MarginEnd = 20,
                RowHomogeneous = true
            };
            var keyMap = new Dictionary<string, Button>();
            Button? currentHighlightedButton = null;
            Button? currentDeadKeyButton = null;
            var btnA = new Gtk.Button { Label = "A", CanFocus = false };
            for (var i = 0; i < rows.Length - 1; i++)
            {
                int displace = i switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 5,
                    4 => 6,
                    _ => 0,
                };
                for (var j = 0; j < rows[i].Length; j++)
                {
                    var keyData = rows[i][j];
                    var keyButton = new Button { Label = keyData.Label, CanFocus = false };
                    grid.Attach(keyButton, j + displace, i, keyData.Width, 1);
                    displace += keyData.Width - 1;
                    var parts = keyData.Label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts) keyMap[part.ToUpper()] = keyButton;
                }
            }
            var leftShift = new Button { Label = rows[4][0].Label, CanFocus = false };
            var rightShift = new Button { Label = rows[4][2].Label, CanFocus = false };
            var space = new Button { Label = rows[4][1].Label, CanFocus = false };
            keyMap["SHIFT"] = leftShift;
            keyMap["SHIFT"] = rightShift;
            keyMap[" "] = space;
            // var rightShift = new Button { Label = rows[4][2].Label, CanFocus = false };
            grid.Attach(leftShift, 0, 4, 8, 1);
            grid.Attach(space, 12, 4, 30, 1);
            grid.Attach(rightShift, 46, 4, 8, 1);


            var typingSection = Box.New(Orientation.Vertical, 20);
            typingSection.Valign = Align.Start;
            typingSection.Halign = Align.Center;
            typingSection.MarginTop = 20;
            var targetEntry = Entry.New();
            var inputEntry = Entry.New();
            targetEntry.SetText("");
            targetEntry.Editable = false;
            targetEntry.CanFocus = false;
            targetEntry.Halign = Align.Fill;
            inputEntry.Halign = Align.Fill;
            targetEntry.MarginStart = 20;
            targetEntry.MarginEnd = 20;
            inputEntry.MarginStart = 20;
            inputEntry.MarginEnd = 20;


            Gio.Menu GetLevels()
            {
                if (!Directory.Exists("Levels")) Directory.CreateDirectory("Levels");
                levels = Directory.GetFiles("Levels/", "*.TXT");
                var levelsSubMenu = Gio.Menu.New();
                foreach (var level in levels)
                {
                    string levelName = Path.GetFileNameWithoutExtension(level);
                    levelsSubMenu.Append(levelName, $"app.openLevel('{level}')");
                }
                return levelsSubMenu;
            }
            void openLevel(string level)
            {
                Console.WriteLine(level);
                lines = System.IO.File.ReadAllLines(level, System.Text.Encoding.Latin1);
                lineCounter = 0;
                adjusting = true;
                targetEntry.SetText(lines[lineCounter]);
                inputEntry.SetText("");
                adjusting = false;
                inputEntry.GrabFocus();
                if (lines[lineCounter].Length > 0) highlightKey(lines[lineCounter][0].ToString());
            }
            var openLevelAction = Gio.SimpleAction.New("openLevel", GLib.VariantType.String);
            openLevelAction.OnActivate += (action, args) =>
            {
                string level = args.Parameter!.GetString(out _);
                openLevel(level);
            };
            app.AddAction(openLevelAction);

            inputEntry.WidthRequest = 800;
            var fontDescription = Pango.FontDescription.FromString("monospace 12");
            var fontAttribute = Pango.AttrFontDesc.New(fontDescription);
            var attributes = Pango.AttrList.New();
            attributes.Insert(fontAttribute);
            targetEntry.Attributes = attributes;
            inputEntry.Attributes = attributes;
            typingSection.Append(targetEntry);
            typingSection.Append(inputEntry);
            typingSection.Append(grid);


            toolbarView.SetContent(typingSection);
            var fileMenuButton = MenuButton.New();
            var optionsMenuButton = MenuButton.New();
            var helpMenuButton = MenuButton.New();

            var fileMenu = Menu.New();
            var optionsMenu = Menu.New();
            var helpMenu = Menu.New();

            var levelSubMenu = Menu.New();
            levelSubMenu = GetLevels();
            openLevel(levels[levelCounter]);

            levelSubMenu.Append("Other...", null);
            fileMenu.AppendSubmenu("Level", levelSubMenu);
            //-------------------------------------
            fileMenu.Append("Scores : view...", "app.scores-view");
            fileMenu.Append("Scores : clear...", "app.scores-clear");
            //-------------------------------------
            fileMenu.Append("Quit", "app.quit");

            optionsMenu.Append("Next line", "app.next-line");
            optionsMenu.Append("Previous line", "app.previous-line");
            optionsMenu.Append("Toggle timer", "app.timer-toggle");
            optionsMenu.Append("Counters : reset", "app.reset-counters");
            optionsMenu.Append("Zoom", null);
            //-------------------------------------
            optionsMenu.Append("Preferences", null);

            helpMenu.Append("Index", null);
            helpMenu.Append("About", null);

            fileMenuButton.MenuModel = fileMenu;
            optionsMenuButton.MenuModel = optionsMenu;
            helpMenuButton.MenuModel = helpMenu;
            fileMenuButton.Label = "File";
            optionsMenuButton.Label = "Options";
            helpMenuButton.Label = "Help";
            topBar.PackStart(fileMenuButton);
            topBar.PackStart(optionsMenuButton);
            topBar.PackStart(helpMenuButton);


            var charLabel = Label.New($"Characters : {characters}");
            var faultLabel = Label.New($"Faults : {faults} ({percentage}%)");
            var timeLabel = Label.New($"Time : {time}s");
            var speedLabel = Label.New($"Speed : {speed} char/s");
            statsBox.Append(charLabel);
            statsBox.Append(faultLabel);
            statsBox.Append(timeLabel);
            statsBox.Append(speedLabel);

            var quitAction = Gio.SimpleAction.New("quit", null);
            quitAction.OnActivate += (_, _) => app.Quit();
            app.AddAction(quitAction);
            app.SetAccelsForAction("app.quit", ["<Alt>x"]);

            var timeToggleAction = Gio.SimpleAction.New("timer-toggle", null);
            timeToggleAction.OnActivate += (_, _) =>
            {
                if (timeStop)
                {
                    timeStop = false;
                    timeLabel.RemoveCssClass("error");
                }
                else
                {
                    timeStop = true;
                    timeLabel.AddCssClass("error");
                }
            };
            app.AddAction(timeToggleAction);
            app.SetAccelsForAction("app.timer-toggle", ["<Primary>h"]);

            var resetCountersAction = Gio.SimpleAction.New("reset-counters", null);
            resetCountersAction.OnActivate += (_, _) =>
            {
                characters = 0;
                faults = 0;
                time = 0;
                speed = 0;
                percentage = 0;
                charLabel.SetText($"Characters : {characters}");
                faultLabel.SetText($"Faults : {faults} ({percentage}%)");
                timeLabel.SetText($"Time : {time}s");
                speedLabel.SetText($"Speed : {speed} char/s");
            };
            app.AddAction(resetCountersAction);
            app.SetAccelsForAction("app.reset-counters", ["<Primary>z"]);


            var nextLineAction = Gio.SimpleAction.New("next-line", null);
            nextLineAction.OnActivate += (_, _) => nextLine();
            app.AddAction(nextLineAction);
            app.SetAccelsForAction("app.next-line", ["<Primary>s"]);

            var previousLineAction = Gio.SimpleAction.New("previous-line", null);
            previousLineAction.OnActivate += (_, _) => previousLine();
            app.AddAction(previousLineAction);
            app.SetAccelsForAction("app.previous-line", ["<Primary>p"]);


            void nextLine()
            {
                lineCounter++;
                if (lineCounter >= lines.Length) { lineCounter = 0; nextLevel(); }
                adjusting = true;
                targetEntry.SetText(lines[lineCounter]);
                inputEntry.SetText("");
                adjusting = false;
                if (lines[lineCounter].Length > 0) highlightKey(lines[lineCounter][0].ToString());
            }
            void nextLevel()
            {
                levelCounter++;
                if (levelCounter >= levels.Length) levelCounter = 0;
                Console.WriteLine(levelCounter);
                openLevel(levels[levelCounter]);
            }

            void previousLine()
            {
                lineCounter--;
                if (lineCounter < 0) { previousLevel(); lineCounter = lines.Length - 1; }
                adjusting = true;
                targetEntry.SetText(lines[lineCounter]);
                inputEntry.SetText("");
                adjusting = false;
                if (lines[lineCounter].Length > 0) highlightKey(lines[lineCounter][0].ToString());

            }
            void previousLevel()
            {
                levelCounter--;
                if (levelCounter < 0) levelCounter = levels.Length - 1;
                Console.WriteLine(levelCounter);
                openLevel(levels[levelCounter]);
            }

            string getBaseKey(char c)
            {
                string s = c.ToString().ToLower();
                if ("àáâãäå".Contains(s)) return "A";
                if ("èéêë".Contains(s)) return "E";
                if ("ìíîï".Contains(s)) return "I";
                if ("òóôõöø".Contains(s)) return "O";
                if ("ùúûü".Contains(s)) return "U";
                if ("ç".Contains(s)) return "C";
                if ("ñ".Contains(s)) return "N";
                return s.ToUpper();
            }

            string? getDeadKey(char c)
            {
                // Return the primary symbol on the physical key
                if ("àèìòùñãõ".Contains(c)) return "`";
                if ("áéíóúýäëïöü".Contains(c)) return "'";
                if ("âêîôû".Contains(c)) return "6";
                return null;
            }

            void toggleShift(bool highlight)
            {
                if (highlight)
                {
                    leftShift.AddCssClass("suggested-action");
                    rightShift.AddCssClass("suggested-action");
                }
                else
                {
                    leftShift.RemoveCssClass("suggested-action");
                    rightShift.RemoveCssClass("suggested-action");
                }
            }

            void highlightKey(string keyName)
            {
                if (string.IsNullOrEmpty(keyName)) return;
                char targetChar = keyName[0];
                string baseKey = getBaseKey(targetChar);
                string? deadKey = getDeadKey(targetChar);
                if (keyMap.TryGetValue(baseKey, out var newButton))
                {
                    currentHighlightedButton?.RemoveCssClass("suggested-action");
                    currentDeadKeyButton?.RemoveCssClass("warning");
                    toggleShift(false);
                    currentHighlightedButton = newButton;
                    currentHighlightedButton.AddCssClass("suggested-action");
                    if (deadKey != null && keyMap.TryGetValue(deadKey.ToUpper(), out var deadButton))
                    {
                        currentDeadKeyButton = deadButton;
                        currentDeadKeyButton.AddCssClass("warning");
                    }
                    else currentDeadKeyButton = null;
                    if (char.IsUpper(targetChar) || "~!@#$%^&*()_+{}|:\"<>?".Contains(targetChar)) toggleShift(true);
                }
            }

            void checkInput()
            {
                string typed = inputEntry.GetText();
                string target = targetEntry.GetText();
                if (target.Length == 0 || typed.Length == 0 || typed.Length > target.Length) return;
                int currentIndex = typed.Length - 1;
                char typedChar = typed[currentIndex];
                char targetChar = target[currentIndex];
                if (typedChar != targetChar)
                {
                    window.GetSurface()?.Beep();
                    adjusting = true;
                    inputEntry.SetText(target[0..currentIndex]);
                    inputEntry.SetPosition(currentIndex);
                    adjusting = false;
                    faults++;
                    percentage = (characters == 0) ? 100 : (int)((float)faults / (characters + faults) * 100);
                    faultLabel.SetText($"Faults : {faults} ({percentage}%)");
                    return;
                }
                else if (typed.Length >= target.Length) nextLine();
                else
                {
                    highlightKey(target[typed.Length].ToString());
                }
                characters++;
            }

            var keyController = Gtk.EventControllerKey.New();
            keyController.SetPropagationPhase(PropagationPhase.Capture);
            inputEntry.AddController(keyController);

            keyController.OnKeyPressed += (sender, args) =>
            {
                if (args.Keyval == Gdk.Constants.KEY_BackSpace) return true;
                return false;
            };
            inputEntry.OnChanged += (sender, args) =>
            {
                timeStop = false;
                if (adjusting) return;
                if (timerId == 0)
                {
                    timerId = GLib.Functions.TimeoutAdd(0, 1000, () =>
                        {
                            if (!timeStop) time++;
                            timeLabel.SetText($"Time : {time}s");
                            speed = (time == 0) ? 0 : 60 * characters / time;
                            speedLabel.SetText($"Speed : {speed} char/min");
                            return true;
                        });
                }
                checkInput();
                percentage = (characters == 0) ? 0 : (int)((float)faults / (characters + faults) * 100);
                faultLabel.SetText($"Faults : {faults} ({percentage}%)");
                charLabel.SetText($"Characters : {characters}");
            };

            statsBox.AddCssClass("footer");
            toolbarView.AddBottomBar(statsBox);
            window.SetContent(toolbarView);
            window.Show();
            inputEntry.GrabFocus();
        };
        return app.RunWithSynchronizationContext(args);
    }
}
