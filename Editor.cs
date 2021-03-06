﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenXP
{
    class Editor
    {
        public static OpenXP Form { get; set; }
        public static Project Project { get; set; } = null;

        public static LayerType ActiveLayer { get; set; }
        public static DrawToolType ActiveDrawTool { get; set; }
        public static ZoomType ActiveZoomType { get; set; }

        public static EditorIni Ini;

        public static bool DimOtherLayers = true;
        public static bool ViewAllLayers = true;

        public static void Exit()
        {
            if(Ini != null) Ini.Save();
            Application.Exit();
        }

        public static void UpdateCaption()
        {
            string caption = "";
            if (Project != null) {
                caption += Project.Ini.Title;
                if (Project.Dirty) caption += "*";
                caption += " - ";
            }
            caption += "OpenXP";
            Form.Text = caption;
        }

        public static void Touch()
        {
            Project.Touch();
            UpdateCaption();
        }

        public static void OnStartup()
        {
            Ini = new EditorIni();
        }
        
        public static void OpenProject()
        {
            string projectFile = null;
            //browse for a project
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.InitialDirectory = Ini.LastProjectDirectory;
            openDialog.Filter = "RPGXP Project (*.rxproj)|*.rxproj|All files (*.*)|*.*";
            openDialog.FilterIndex = 1;
            openDialog.RestoreDirectory = true;
            openDialog.Multiselect = false;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(openDialog.FileName))
                    {
                        projectFile = openDialog.FileName;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    UpdateCaption();
                    return;
                }
                if (projectFile != null)
                {
                    if (File.Exists(projectFile))
                    {
                        Project = new Project();
                        Project.Path = new FileInfo(projectFile).FullName;
                        Project.Directory = new FileInfo(projectFile).Directory.FullName;
                        if (Directory.Exists(Project.Directory))
                        {
                            string msg = Project.Setup();
                            if (string.IsNullOrWhiteSpace(msg)){
                                //all good
                                Ini.LastProjectDirectory = Project.Directory;
                                EnableControls();
                                UpdateCaption();
                                return;
                            } else
                            {
                                //there was an error during project setup
                                MessageBox.Show(msg);
                                Project = null;
                                UpdateCaption();
                                return;
                            }
                        }
                    }
                }
                //If we are still here, something has gone wrong.
                Project = null;
                MessageBox.Show("Error: Could not read locate project data.");
                UpdateCaption();
                return;
            }
        }

        public static void NewProject()
        {
            //UpdateCaption(form);
            //EnableControls(form);
        }

        public static void SaveProject()
        {
            if (Project != null) Project.Save();
            UpdateCaption();
        }

        //can also be used to refresh the editor
        //returns true if no project is active afterwards
        public static bool CloseProject()
        {
            if (Project != null)
            {
                //todo: flush any project-related stuff
                //todo: prompt for save, return false if cancelled
                Project = null;
                ChangeLayer(LayerType.EVENTS);
                DisableControls();
                UpdateCaption();
                return true;
            }
            else return true;
        }

        public static void ChangeDrawTool(DrawToolType newTool)
        {
            if(newTool != ActiveDrawTool)
            {
                ActiveDrawTool = newTool;
                //update editor selection display
                Form.updateSelectedDrawTool(ActiveDrawTool);
            }
        }

        public static void ToggleDimOtherLayers()
        {
            DimOtherLayers = !DimOtherLayers;
            //toggle the checked state of the menu item
            Form.SetDimOtherLayersChecked(DimOtherLayers);
        }

        public static void SetViewAllLayers(bool state)
        {
            ViewAllLayers = state;
        }

        public static void ChangeZoom(ZoomType newZoom)
        {
            if(newZoom != ActiveZoomType)
            {
                ActiveZoomType = newZoom;
                Form.RepaintMap();
            }
        }

        public static void ChangeLayer(LayerType newLayer)
        {
            if(newLayer != ActiveLayer)
            {
                ActiveLayer = newLayer;
                if(ActiveLayer == LayerType.EVENTS)
                {
                    Form.disableDrawTools();
                    
                } else
                {
                    Form.enableDrawTools();
                }
                //update editor selection display
                Form.updateSelectedLayer(ActiveLayer);
                //force a repaint on the map
                Form.RepaintMap();
            }
        }

        public static void Playtest()
        {
            string command = Path.Combine(Project.Directory, @"Game.exe");
            string args = "debug";

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = args;
            process.Start();
        }

        //call when a project is loaded/enabled to enable toolbars/menus
        public static void EnableControls()
        {
            Form.enableControls();
        }

        //call when a project is unloaded/disabled to disable toolbars/menus
        public static void DisableControls()
        {
            Form.disableControls();
        }

        public static void SelectMap(int id)
        {
            Form.SelectMap(id);
        }

        public static int GetSelectedMapId()
        {
            return Form.GetMapId();
        }
    }
}
