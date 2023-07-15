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

    enum COLOR_PALETTE
    {
        // This four have to match the csys item type enum.

        COL_COMMAND = 0,    //!< Color for command logs
        COL_LOG,            //!< Color for in-command logs
        COL_WARNING,        //!< Color for warnings logs
        COL_ERROR,          //!< Color for error logs
        COL_INFO,            //!< Color for info logs

        COL_TIMESTAMP,      //!< Color for timestamps

        COL_COUNT            //!< For bookkeeping purposes
    };

    private const float inputBufferSize = 1024;
    private float m_WindowAlpha = 1f;
    public string ConsoleName = "DefaultConsole";
    private bool m_IsConsoleOpened;
    private bool m_ColoredOutput;
    private bool m_AutoScroll;
    private bool m_FilterBar;
    private bool m_TimeStamps;
    private bool m_ScrollToBottom;
    private bool m_WasPrevFrameTabCompletion;
    private readonly Vector4[] m_ColorPalette = new Vector4[(int)COLOR_PALETTE.COL_COUNT];
    ImGuiTextFilter m_TextFilter;    //!< Logging filer


    private CircularBuffer<WidgetQonsoleController.LogEntry> _items;


    public void SetItems(CircularBuffer<WidgetQonsoleController.LogEntry> items)
    {
        _items = items;
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


    void Draw()
    {
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

        //// Section off.
        ImGui.Separator();

        // Command-line 

        InputBar();

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
                ImGui.ColorEdit4("Command##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_COMMAND], flags);
                ImGui.ColorEdit4("Log##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_LOG], flags);
                ImGui.ColorEdit4("Warning##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_WARNING], flags);
                ImGui.ColorEdit4("Error##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_ERROR], flags);
                ImGui.ColorEdit4("Info##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_INFO], flags);
                ImGui.ColorEdit4("Time Stamp##", ref m_ColorPalette[(int)COLOR_PALETTE.COL_TIMESTAMP], flags);
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
        m_FilterBar = true;
        m_TimeStamps = true;

        // Style
        m_WindowAlpha = 1;
        m_ColorPalette[(int)COLOR_PALETTE.COL_COMMAND] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        m_ColorPalette[(int)COLOR_PALETTE.COL_LOG] = new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
        m_ColorPalette[(int)COLOR_PALETTE.COL_WARNING] = new Vector4(1.0f, 0.87f, 0.37f, 1.0f);
        m_ColorPalette[(int)COLOR_PALETTE.COL_ERROR] = new Vector4(1.0f, 0.365f, 0.365f, 1.0f);
        m_ColorPalette[(int)COLOR_PALETTE.COL_INFO] = new Vector4(0.46f, 0.96f, 0.46f, 1.0f);
        m_ColorPalette[(int)COLOR_PALETTE.COL_TIMESTAMP] = new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
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

    void LogWindow()
    {
        float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.y + ImGui.GetFrameHeightWithSpacing();
        if (ImGui.BeginChild("ScrollRegion##", new Vector2(0, -footerHeightToReserve), false, 0))
        {
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
                    //ImGui.PushStyleColor(ImGuiCol.Text, m_ColorPalette[item.m_Type]);
                    //ImGui.TextUnformatted(item.Get().data());
                    ImGui.TextUnformatted(item.Message);
                    //ImGui.PopStyleColor();
                }
                else
                {
                    //ImGui.TextUnformatted(item.Get().data());
                    ImGui.TextUnformatted(item.Message);
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


    void InputBar()
    {
        // Variables.
        //ImGuiInputTextFlags inputTextFlags = ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackCharFilter | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways;

        // Only reclaim after enter key is pressed!
        bool reclaimFocus = false;

        // Input widget. (Width an always fixed width)
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
        ImGui.PopItemWidth();


        // Reset suggestions when client provides char input.
        if (ImGui.IsItemEdited() && !m_WasPrevFrameTabCompletion)
        {
            //m_CmdSuggestions.clear();
        }
        m_WasPrevFrameTabCompletion = false;

        // Auto-focus on window apparition
        ImGui.SetItemDefaultFocus();
        if (reclaimFocus)
            ImGui.SetKeyboardFocusHere(-1); // Focus on command line after clearing.
    }
}
