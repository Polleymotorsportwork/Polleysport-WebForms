﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

public partial class AdminPages_AdminGalleryManagement : System.Web.UI.Page
{
    DataTable dt;
    private string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            CurrentPage = 0;
        }

        BindGrid();
    }

    #region Admin CRUD Grid
    public void BindGrid()
    {
        try
        {
            //Fetch data from mysql database
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            
            string cmd = "SELECT [GalleryID] ,[GalleryTitle],[GalleryContent],[GalleryImageURL],[enabled] FROM[polleymo_database].[dbo].[Gallery]";
            SqlDataAdapter dAdapter = new SqlDataAdapter(cmd, conn);
            DataSet ds = new DataSet();
            dAdapter.Fill(ds);
            dt = ds.Tables[0];

            //Bind the fetched data to gridview
            GridView1.DataSource = dt;
            GridView1.DataBind();
            conn.Close();

        }
        catch (SqlException ex)
        {
            System.Console.Error.Write(ex.Message);

        }

    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        SqlConnection con = new SqlConnection(connString);
        con.Open();

        SqlDataAdapter adapt = new SqlDataAdapter("SELECT * FROM Gallery WHERE GalleryTitle LIKE '%" + txtSearch.Text + "%'", con);
        //dt = new DataTable();
        DataSet ds = new DataSet();
        adapt.Fill(ds);
        
        dt = ds.Tables[0];
        GridView1.DataSource = dt;
        GridView1.DataBind();
        con.Close();   
        //dt.DefaultView.RowFilter = string.Format("GalleryTitle LIKE '{0}'", txtSearch.Text);
        //GridView1.DataBind();
        //GridView1.DataSource = dt.DefaultView.RowFilter = string.Format("GalleryTitle LIKE '{0}'", txtSearch.Text);
        //GridView1.DataBind();
    }

    protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int index = Convert.ToInt32(e.CommandArgument);
        if (e.CommandName.Equals("detail"))
        {
            int galleryId = Convert.ToInt32(GridView1.DataKeys[index].Value);
            IEnumerable<DataRow> query = from i in dt.AsEnumerable()
                                         where i.Field<Int32>("GalleryID").Equals(galleryId)
                                         select i;
            DataTable detailTable = query.CopyToDataTable<DataRow>();
            //DetailsView1.DataSource = detailTable;
            //DetailsView1.DataBind();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("$('#detailModal').modal('show');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "DetailModalScript", sb.ToString(), false);
        }
        else if (e.CommandName.Equals("editRecord"))
        {
            GridViewRow gvrow = GridView1.Rows[index];
            lblGalleryID.Text = HttpUtility.HtmlDecode(gvrow.Cells[2].Text);
            txtgalleryTitle.Text = HttpUtility.HtmlDecode(gvrow.Cells[3].Text);
            txtXtext.Text = HttpUtility.HtmlDecode(gvrow.Cells[4].Text);
            //txtStock.Text = HttpUtility.HtmlDecode(gvrow.Cells[5].Text);
            //txtPrice.Text = HttpUtility.HtmlDecode(gvrow.Cells[6].Text);
            hfImageURL.Value = HttpUtility.HtmlDecode(gvrow.Cells[5].Text);
            rblProductActive.SelectedValue = HttpUtility.HtmlDecode(gvrow.Cells[6].Text);

            lblResult.Visible = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("$('#editModal').modal('show');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "EditModalScript", sb.ToString(), false);

        }
        else if (e.CommandName.Equals("deleteRecord"))
        {
            string galleryId = GridView1.DataKeys[index].Value.ToString();
            hfGalleryID.Value = galleryId;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("$('#deleteModal').modal('show');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "DeleteModalScript", sb.ToString(), false);
        }

    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        int galleryId = Convert.ToInt32(lblGalleryID.Text);
        string galleryTitle = txtgalleryTitle.Text;
        string description = txtXtext.Text;
        string imageUrl = string.Empty;
        if (fileUploadImage.HasFile)
        {
            imageUrl = StartUpLoad(fileUploadImage);
        }
        //int categoryId = Convert.ToInt32(ddCatId.SelectedValue);
        //int subCategoryId = Convert.ToInt32(ddSubCatId.SelectedValue);
        int enabled = Convert.ToInt32(rblProductActive.SelectedValue);
        executeUpdate(galleryTitle, description, imageUrl, enabled, galleryId);
        BindGrid();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(@"<script type='text/javascript'>");
        sb.Append("alert('Records Updated Successfully');");
        sb.Append("$('#editModal').modal('hide');");
        sb.Append(@"</script>");
        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "EditHideModalScript", sb.ToString(), false);

    }

    private void executeUpdate(string galleryTitle, string description, string imageUrl, int enabled, int galleryId)
    {
        string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        try
        {
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();

            string updatecmd = "UPDATE Gallery SET GalleryTitle = @Name, GalleryContent = @Description, GalleryImageURL = @ImageURL, enabled = @enabled WHERE GalleryID = @GalleryID";
            SqlCommand updateCmd = new SqlCommand(updatecmd, conn);
            updateCmd.Parameters.AddWithValue("@Name", galleryTitle);
	        updateCmd.Parameters.AddWithValue("@Description", description);
	        updateCmd.Parameters.AddWithValue("@ImageURL", imageUrl);
            updateCmd.Parameters.AddWithValue("@enabled", enabled);

            updateCmd.Parameters.AddWithValue("@GalleryID", galleryId);
	        
            updateCmd.ExecuteNonQuery();
            conn.Close();

        }
        catch (SqlException me)
        {
            System.Console.Error.Write(me.InnerException.Data);
        }
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(@"<script type='text/javascript'>");
        sb.Append("$('#addModal').modal('show');");
        sb.Append(@"</script>");
        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "AddShowModalScript", sb.ToString(), false);

    }

    protected void btnAddRecord_Click(object sender, EventArgs e)
    {
        string galleryName = txtGalleryName.Text;
        string description = txtDescription.Text;
        
        if(insertUploadPicture.HasFile)
        {
            StartUpLoad(insertUploadPicture);
        }
        //StartUpLoad();
        string imageUrl = insertUploadPicture.FileName;// StartUpLoad(insertUploadPicture);

        executeAdd(galleryName, description, imageUrl);
        BindGrid();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(@"<script type='text/javascript'>");
        sb.Append("alert('Record Added Successfully');");
        sb.Append("$('#addModal').modal('hide');");
        sb.Append(@"</script>");
        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "AddHideModalScript", sb.ToString(), false);


    }

    private void executeAdd(string galleryName, string description, string imageUrl)
    {
        string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        try
        {
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();

            string updatecmd = "INSERT INTO Gallery (GalleryTitle,GalleryContent,GalleryImageURL,[enabled]) VALUES (@GalleryTitle, @GalleryContent,@GalleryImageURL,1 )";
            SqlCommand addCmd = new SqlCommand(updatecmd, conn);
            addCmd.Parameters.AddWithValue("@GalleryTitle", galleryName);
            addCmd.Parameters.AddWithValue("@GalleryContent", description);
            addCmd.Parameters.AddWithValue("@GalleryImageURL", imageUrl);

            addCmd.ExecuteNonQuery();
            conn.Close();

        }
        catch (SqlException me)
        {
            System.Console.Write(me.Message);
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        int galleryId = int.Parse(hfGalleryID.Value);
        executeDelete(galleryId);
        BindGrid();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(@"<script type='text/javascript'>");
        sb.Append("alert('Record deleted Successfully');");
        sb.Append("$('#deleteModal').modal('hide');");
        sb.Append(@"</script>");
        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "delHideModalScript", sb.ToString(), false);


    }

    private void executeDelete(int galleryId)
    {
        string connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        try
        {
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            //string updatecmd = "delete from tblCountry where Code=@code";
            string updatecmd = "UPDATE Gallery SET [enabled] = 0 WHERE GalleryID=@GalleryID";
            SqlCommand addCmd = new SqlCommand(updatecmd, conn);
            addCmd.Parameters.AddWithValue("@GalleryID", galleryId);
            addCmd.ExecuteNonQuery();
            conn.Close();

        }
        catch (SqlException me)
        {
            System.Console.Write(me.Message);
        }

    }

    #endregion

    #region Pagination controls

    protected void OnPageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridView1.PageIndex = e.NewPageIndex;
        GridView1.DataBind();
    }

    public int CurrentPage
    {
        get
        {
            // look for current page in ViewState
            object o = ViewState["_CurrentPage"];
            if (o == null)
                return 0;
            return (int)o;
        }
        set { ViewState["_CurrentPage"] = value; }
    }

    public void btnNext_Click(object sender, System.EventArgs e)
    {
        // Set viewstate variable to the next page
        CurrentPage += 1;
        // Reload control
        //BindPagedDataSource();
        BindGrid();
    }

    public void btnPrev_Click(object sender, System.EventArgs e)
    {
        // Set viewstate variable to the previous page
        if (CurrentPage > 0)
            CurrentPage -= 1;
        else
            CurrentPage = 1;
        // Reload control
        //BindPagedDataSource();
        BindGrid();
    }
    #endregion

    private string StartUpLoad(FileUpload UploadPicture)
    {
        //get the file name of the posted image
        string imgName = UploadPicture.FileName;

        //get the size in bytes that
        int imgSize = UploadPicture.PostedFile.ContentLength;

        //validates the posted file before saving
        if (UploadPicture.PostedFile != null && imgName != "")
        {
            // 10240 KB means 10MB, You can change the value based on your requirement
            if (imgSize > 20240)
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(Page), "Alert", "alert('File is too big.')", true);

            }
            else
            {
                //For live
                string imagePath = Server.MapPath("~/GalleryImages/");
                imagePath = imagePath + @"\" + imgName;

                //For testing
                //string imagePath = ConfigurationManager.AppSettings["UploadPath"] + imgName;

                //then save it to the Folder
                UploadPicture.SaveAs(imagePath);

                //ImageResizeUtils.ResizeImage(imagePath, 300, 300);
                Page.ClientScript.RegisterClientScriptBlock(typeof(Page), "Alert", "alert('Image saved!')", true);
                //ProductImages/imgName
                //hfImageURL.Value = "GalleryImages/" + imgName;
            }
        }
        return "GalleryImages/" + imgName;
    }

}