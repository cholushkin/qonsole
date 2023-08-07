using System;
using System.Collections.Generic;
using Gamelib.DataStructures;
using ImGuiNET;
using Qonsole;
using UImGui;
using UnityEngine;


// todo: port visual from c++
// todo: rename all vairalbes to local naming convention
public class WidgetQonsoleView : MonoBehaviour
{
    public class EventConsoleEnteredText
    {
        public string Text;
    }

    public class EventConsoleTextChange
    {
        public string Text;
    }

    [Serializable]
    public class ColorPalette
    {
        public Color Error;
        public Color Assert;
        public Color Exception;
        public Color Warning;
        public Color Log;
    }

    public bool ResetPosition;
    public ColorPalette TextColorPalette;
    public int HistoryBoxNumLinesMax = 8;
    public int SuggestionBoxNumLinesMax = 8;
    public float HistoryBoxAlpha = 0.75f;

    private const float inputBufferSize = 1024;
    public float m_WindowAlpha = 1f;
    public string ConsoleName = "DefaultConsole";
    private bool m_IsConsoleOpened;
    private bool m_ColoredOutput;
    private bool m_MessageSeparator;
    private bool m_AutoScroll;
    private bool m_FilterBar;
    private bool m_TimeStamps;
    private bool m_ScrollToBottom;
    private bool m_WasPrevFrameTabCompletion;
    ImGuiTextFilter m_TextFilter;    //!< Logging filer



    private CircularBuffer<WidgetQonsoleController.LogEntry> _items;
    private CircularBuffer<string> _historyBuffer;
    private int _historyBufferPointer;
    private List<ConsoleSystem.ConsoleMethodInfo> _cmdSuggestionList;


    // todo: rename
    public void SetItems(CircularBuffer<WidgetQonsoleController.LogEntry> items)
    {
        _items = items;
    }

    public void SetHistoryBuffer(CircularBuffer<string> historyBuffer)
    {
        _historyBuffer = historyBuffer;
    }

    public void SetCmdSuggestionList(List<ConsoleSystem.ConsoleMethodInfo> cmdSuggestionList)
    {
        _cmdSuggestionList = cmdSuggestionList;
    }


    void Awake()
    {
        // Set input buffer size.
        //m_Buffer.resize(inputBufferSize);
        //m_HistoryIndex = std::numeric_limits < size_t >::min();

        //// Specify custom data to be store/loaded from imgui.ini
        //InitIniSettings();

        //// Set Console ImGui default settings
        //if (!m_LoadedFromIni)
        //{
        DefaultSettings();
        //}

        //// Custom functions.
        //RegisterConsoleCommands();
    }

    private void OnEnable()
    {
        UImGuiUtility.Layout += OnLayout;
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
    }

    private void OnLayout(UImGui.UImGui uImGui)
    {
        Draw();
    }

    private bool suggestionBoxPopup = false;

    void Draw()
    {
	    if(ResetPosition)
		    ImGui.SetNextWindowPos(Vector2.zero);

        // Begin Console Window.
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, m_WindowAlpha);
        if (!ImGui.Begin(ConsoleName, ref m_IsConsoleOpened, ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleVar();
            ImGui.End();
            return;
        }
        ImGui.PopStyleVar();

        // Menu bar 
        MenuBar();

        // Filter bar
        if (m_FilterBar)
            FilterBar();

        // Console Logs
        LogWindow();

        // Section off.
        ImGui.Separator();

        // Command-line 
        InputBar();
        PopupHistorybox();

        if (!string.IsNullOrEmpty(inputBuffer))
        {
            //if (!suggestionBoxPopup)
            //{
            //    ImGui.OpenPopup("SuggestionBoxPopup");
            //    suggestionBoxPopup = true;
            //}

            PopupSuggestionBox();
        }


        ImGui.End();
    }

    void MenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            // Settings menu.
            if (ImGui.BeginMenu("Settings"))
            {
                // Colored output
                ImGui.Checkbox("Colored Output", ref m_ColoredOutput);
                ImGui.SameLine();
                HelpMaker("Enable colored command output");

                // Message separator line
                ImGui.Checkbox("Message Separator", ref m_MessageSeparator);
                ImGui.SameLine();
                HelpMaker("Enable message separator");

                // Auto Scroll
                ImGui.Checkbox("Auto Scroll", ref m_AutoScroll);
                ImGui.SameLine();
                HelpMaker("Automatically scroll to bottom of console log");

                // Filter bar
                ImGui.Checkbox("Filter Bar", ref m_FilterBar);
                ImGui.SameLine();
                HelpMaker("Enable console filter bar");

                // Time stamp
                ImGui.Checkbox("Time Stamps", ref m_TimeStamps);
                ImGui.SameLine();
                HelpMaker("Display command execution timestamps");

                // Reset to default settings
                if (ImGui.Button("Reset settings", new Vector2(ImGui.GetColumnWidth(), 0)))
                    ImGui.OpenPopup("Reset Settings?");

                // Confirmation
                bool unused = true;
                if (ImGui.BeginPopupModal("Reset Settings?", ref unused, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("All settings will be reset to default.\nThis operation cannot be undone!\n\n");
                    ImGui.Separator();

                    if (ImGui.Button("Reset", new Vector2(120, 0)))
                    {
                        DefaultSettings();
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SetItemDefaultFocus();
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel", new Vector2(120, 0)))
                    { ImGui.CloseCurrentPopup(); }
                    ImGui.EndPopup();
                }

                ImGui.EndMenu();
            }

            // View settings.
            if (ImGui.BeginMenu("Appearance"))
            {
                // Logging Colors
                ImGuiColorEditFlags flags =
                        ImGuiColorEditFlags.Float | ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar;

                ImGui.TextUnformatted("Color Palette");
                ImGui.Indent();

                //ImGui.ColorEdit4("Command##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_COMMAND], flags);
                //ImGui.ColorEdit4("Log##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_LOG], flags);
                //ImGui.ColorEdit4("Warning##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_WARNING], flags);
                //ImGui.ColorEdit4("Error##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_ERROR], flags);
                //ImGui.ColorEdit4("Info##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_INFO], flags);
                //ImGui.ColorEdit4("Time Stamp##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_TIMESTAMP], flags);
                ImGui.Unindent();

                ImGui.Separator();

                // Window transparency.
                ImGui.TextUnformatted("Background");
                ImGui.SliderFloat("Transparency##", ref m_WindowAlpha, 0.1f, 1.0f);

                ImGui.EndMenu();
            }

            // All scripts.
            if (ImGui.BeginMenu("Scripts"))
            {
                // Show registered scripts.
                //    for (const auto &scr_pair : m_ConsoleSystem.Scripts())
                //{
                //        if (ImGui.MenuItem(scr_pair.first.c_str()))
                //        {
                //            m_ConsoleSystem.RunScript(scr_pair.first);
                //            m_ScrollToBottom = true;
                //        }
                //    }

                //    // Reload scripts.
                //    ImGui.Separator();
                //    if (ImGui.Button("Reload Scripts", new Vector2(ImGui.GetColumnWidth(), 0)))
                //    {
                //        for (const auto &scr_pair : m_ConsoleSystem.Scripts())
                //    {
                //            scr_pair.second->Reload();
                //        }
                //    }
                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }


    void DefaultSettings()
    {
        // Settings
        m_AutoScroll = true;
        m_ScrollToBottom = false;
        m_ColoredOutput = true;
        m_MessageSeparator = true;
        m_FilterBar = true;
        m_TimeStamps = true;

        // Style
        m_WindowAlpha = 1;
    }

    void HelpMaker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    void FilterBar()
    {
        //m_TextFilter.Draw("Filter", ImGui::GetWindowWidth() * 0.25f);
        ImGui.Separator();
    }

    private Vector2 _logWindowSize;
    void LogWindow()
    {
        float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.y + ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginChild("ScrollRegion##", new Vector2(0, -footerHeightToReserve), false, ImGuiWindowFlags.NoNavFocus))
        {
            _logWindowSize = ImGui.GetContentRegionAvail();
            // Display colored command output.
            float timestamp_width = ImGui.CalcTextSize("00:00:00:0000").x;    // Timestamp. // todo: move outside and calc 1 time
            int count = 0;                                                                       // Item count.

            // Wrap items.
            ImGui.PushTextWrapPos();

            // Display items.
            foreach (var item in _items)
            {
                //// Exit if word is filtered.
                //if (!m_TextFilter.PassFilter(item.Get().c_str()))
                //    continue;

                // Spacing between commands.
                //if (item.m_Type == csys::COMMAND)
                //{
                //    if (m_TimeStamps) ImGui.PushTextWrapPos(ImGui.GetColumnWidth() - timestamp_width);    // Wrap before timestamps start.
                //    if (count++ != 0) ImGui.Dummy(new Vector2(-1, ImGui.GetFontSize()));                            // No space for the first command.
                //}

                // Items.
                if (m_ColoredOutput)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, LogTypeToColor(item.LogType));
                    ImGui.TextUnformatted(item.Message);
                    if (m_MessageSeparator)
                        ImGui.Separator();
                    //ImGui.SameLine();
                    //ImGUIExtension.HelpMarker("aaaa");
                    ImGui.PopStyleColor();
                }
                else
                {
                    //ImGui.TextUnformatted(item.Get().data());
                    ImGui.TextUnformatted(item.Message);
                    if (m_MessageSeparator)
                        ImGui.Separator();
                }


                // Time stamp.
                //if (item.m_Type == csys::COMMAND && m_TimeStamps)
                //{
                //    // No wrap for timestamps
                //    ImGui.PopTextWrapPos();

                //    // Right align.
                //    ImGui.SameLine(ImGui.GetColumnWidth(-1) - timestamp_width);

                //    // Draw time stamp.
                //    ImGui.PushStyleColor(ImGuiCol_Text, m_ColorPalette[COL_TIMESTAMP]);
                //    ImGui.Text("%02d:%02d:%02d:%04d", ((item.m_TimeStamp / 1000 / 3600) % 24), ((item.m_TimeStamp / 1000 / 60) % 60),
                //                ((item.m_TimeStamp / 1000) % 60), item.m_TimeStamp % 1000);
                //    ImGui.PopStyleColor();

                //}
            }

            // Stop wrapping since we are done displaying console items.
            ImGui.PopTextWrapPos();

            // Auto-scroll logs.
            if ((m_ScrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || m_AutoScroll)))
                ImGui.SetScrollHereY(1.0f);
            m_ScrollToBottom = false;

            // Loop through command string vector.
            ImGui.EndChild();
        }
    }

    private string inputBuffer = "";
    private string prevInputBuffer = "";


    private Vector2 _inputBarScreenPos;
    bool reclaimFocus = false;
    private bool popupFirstFrame = false;
    void InputBar()
    {
        // Variables.
        //ImGuiInputTextFlags inputTextFlags = ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackCharFilter | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways;

        // Only reclaim after enter key is pressed!


        // Input widget. (Width an always fixed width)
        _inputBarScreenPos = ImGui.GetCursorScreenPos();
        ImGui.PushItemWidth(-128);
        if (ImGui.InputText("Input", ref inputBuffer, 128, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            // Validate.
            if (!string.IsNullOrEmpty(inputBuffer))
            {
                // Run command line input.
                GlobalEventAggregator.EventAggregator.Publish(new EventConsoleEnteredText { Text = inputBuffer });
                //m_ConsoleSystem.RunCommand(m_Buffer);

                // Scroll to bottom after its ran.
                m_ScrollToBottom = true;
            }

            //// Keep focus.
            reclaimFocus = true;

            //// Clear command line.
            inputBuffer = "";
        }
        else if (ImGui.IsItemFocused() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Up arrow press in edit box
            popupFirstFrame = true;
            ImGui.OpenPopup("HistoryBoxPopup");
        }
        else if (ImGui.IsWindowHovered())
        {
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                // Right click in edit box
            }
        }
        
        // handle text change
        if (inputBuffer != prevInputBuffer)
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventConsoleTextChange{ Text = inputBuffer });

        }


        ImGui.PopItemWidth();

        prevInputBuffer = inputBuffer;

        // Reset suggestions when client provides char input.
        if (ImGui.IsItemEdited() && !m_WasPrevFrameTabCompletion)
        {
            //m_CmdSuggestions.clear();
        }
        m_WasPrevFrameTabCompletion = false;

        // Auto-focus on window apparition
        ImGui.SetItemDefaultFocus();
        if (reclaimFocus)
        {
            ImGui.SetKeyboardFocusHere(); // Focus on command line after clearing.
            reclaimFocus = false;
        }
    }


    bool PopupHistorybox()
    {
        int popupNumLines = Math.Min(_historyBuffer.Count, HistoryBoxNumLinesMax);
        if (popupNumLines == 0)
            return false;
        float lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var popupSize = new Vector2(Mathf.Max(_logWindowSize.x - 100f, 128f), lineHeight * popupNumLines + 20); // 20 for padding

        // Close popup if the parent window is too small
        if (popupSize.x + 32 > _logWindowSize.x) // 32 is offset 
            return false;
        if (popupSize.y + 32 > _logWindowSize.y)
            return false;


        ImGui.SetNextWindowPos(_inputBarScreenPos - Vector2.up * (popupSize.y + 20f)); // 20 for offset from inputField
        ImGui.SetNextWindowSize(popupSize);
        ImGui.SetNextWindowBgAlpha(HistoryBoxAlpha);

        if (ImGui.BeginPopupContextItem("HistoryBoxPopup"))
        {
            ImGui.Indent();
            var index = 0;

            foreach (var hLine in _historyBuffer)
            {
                if (ImGui.Selectable($"{hLine}##{index}"))
                {
                    inputBuffer = hLine;
                    reclaimFocus = true;
                }

                if (ImGui.IsItemFocused() && Input.GetKeyDown(KeyCode.Return))
                {
                    inputBuffer = hLine;
                    reclaimFocus = true;
                    ImGui.CloseCurrentPopup();
                }

                if (_historyBuffer.Count - 1 == index)
                {
                    ImGui.SetKeyboardFocusHere();
                }
                ++index;
            }

            if (popupFirstFrame)
            {
                popupFirstFrame = false;
                ImGui.SetScrollHereY();
            }

            ImGui.Unindent();
            
            ImGui.EndPopup();

            return true;
        }
        return false;
    }

    bool PopupSuggestionBox()
    {
        int popupNumLines = Math.Min(_cmdSuggestionList.Count, SuggestionBoxNumLinesMax);
        if (popupNumLines == 0)
            return false;
        float lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var popupSize = new Vector2(Mathf.Max(_logWindowSize.x - 100f, 128f), lineHeight * popupNumLines + 20); // 20 for padding

        // Close popup if the parent window is too small
        if (popupSize.x + 32 > _logWindowSize.x) // 32 is offset 
            return false;
        if (popupSize.y + 32 > _logWindowSize.y)
            return false;


        ImGui.SetNextWindowPos(_inputBarScreenPos - Vector2.up * (popupSize.y + 20f)); // 20 for offset from inputField
        //ImGui.SetNextWindowSize(popupSize);
        ImGui.SetNextWindowBgAlpha(HistoryBoxAlpha);


        if (ImGui.BeginChild("qwe", popupSize, true, ImGuiWindowFlags.ChildWindow|ImGuiWindowFlags.NavFlattened)) 
        //if (ImGui.BeginPopupContextItem("SuggestionBoxPopup"))
        //ImGui.BeginTooltip(); // can't get mouse input, no navigation
        {
            ImGui.Indent();
            var index = 0;

            foreach (var suggestionMethodInfo in _cmdSuggestionList)
            {
                var hLine = suggestionMethodInfo.Signature;
                if (ImGui.Selectable($"{hLine}##{index}"))
                {
                    inputBuffer = hLine;
                    reclaimFocus = true;
                }

                if (ImGui.IsItemFocused() && Input.GetKeyDown(KeyCode.Return))
                {
                    inputBuffer = hLine;
                    reclaimFocus = true;
                    ImGui.CloseCurrentPopup();
                }

                ++index;
            }

            //if (popupFirstFrame)
            //{
            //    popupFirstFrame = false;
            //    ImGui.SetScrollHereY();
            //}

            ImGui.Unindent();
            //ImGui.EndPopup();
            ImGui.EndChild();
            return true;
        }
        return false;
    }


    private Color LogTypeToColor(LogType logType)
    {
        if (logType == LogType.Exception)
            return TextColorPalette.Exception;
        if (logType == LogType.Assert)
            return TextColorPalette.Assert;
        if (logType == LogType.Error)
            return TextColorPalette.Error;
        if (logType == LogType.Warning)
            return TextColorPalette.Warning;
        if (logType == LogType.Log)
            return TextColorPalette.Log;

        return Color.white;
    }
}

