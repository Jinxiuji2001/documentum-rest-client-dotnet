﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.Http.Utility;
using Emc.Documentum.Rest.Net;
using System.IO;
using System.Drawing;
using Emc.Documentum.Rest.DataModel.D2;

namespace AspNetWebFormsRestConsumer
{
    public partial class Contact : Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Global.GetRepository() == null) Response.Redirect("Default.aspx");
            D2Repository repository = Global.GetRepository();
            if (!repository.isD2Rest())
            {
                NoteLabel2.ForeColor = Color.OrangeRed;
                NoteLabel2.Text = String.Format("The target repository '{0}' is NOT a D2 repository.", repository.Name);
                btnCreate.Enabled = false;
                return;
            }

            NoteLabel2.ForeColor = Color.LightGreen;
            NoteLabel2.Text = String.Format("The target repository '{0}' is a D2 repository.", repository.Name);     
        }


        protected void btnCreate_Click(object sender, EventArgs e)
        {
            D2Document doc = null;
            D2Repository repository = Global.GetRepository();
            if (String.IsNullOrEmpty(txtName.Text))
            {
                lblError.Visible = true;
                lblError.Text = "Name is required";
                lblError.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                lblError.Text = "";
                lblError.Visible = false;
                if (txtPath.Text.Contains("/"))
                {
                    lblError.Visible = true;
                    lblError.Text = "Is a path";
                    Folder saveToFolder = repository.GetOrCreateFolderByPath(txtPath.Text);

                    if (!String.IsNullOrEmpty(fileToUpload.FileName))
                    {
                        string trailingPath = Path.GetFileName(fileToUpload.PostedFile.FileName);
                        string fullPath = Path.Combine(Server.MapPath(" "), trailingPath);
                        fileToUpload.SaveAs(fullPath);
                        FileInfo tmpFile = new FileInfo(fullPath);
                        //doc = repository.ImportNewD2Document(tmpFile, txtName.Text, txtPathOrProfile.Text);
                        D2Configuration d2Config = new D2Configuration();
                        d2Config.StartVersion = Double.Parse(txtStartVersion.Text);
                        doc = repository.ImportNewD2Document(tmpFile, txtName.Text, txtPath.Text, d2Config);
                        doc.SetPropertyValue("object_name", txtName.Text);
                        doc.SetPropertyValue("title", txtTitle.Text);
                        doc.SetPropertyValue("subject", txtSubject.Text);
                        doc.Save();
                        doc = doc.Fetch<D2Document>();
                        lblError.ForeColor = System.Drawing.Color.Green;
                        lblError.Text = "Saved: \n" + doc.ToString();
                        tmpFile.Delete();
                    }
                }
                else
                {
                    lblError.Visible = true;
                    lblError.Text = "Is a profile";
                }      
            }
        }

        protected void txtPathOrProfile_TextChanged(object sender, EventArgs e)
        {

        }
    }
}