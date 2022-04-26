using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RT
{
    public partial class Form1 : Form
    {
        List<OwnMarker> OMCollection = new List<OwnMarker>();
        GMapMarker selectedMarker;
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            
        }
        private void gMapControl1_Load(object sender, EventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = AccessMode.ServerAndCache; 
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl1.MinZoom = 2; 
            gMapControl1.MaxZoom = 16; 
            gMapControl1.Zoom = 4; 
            gMapControl1.Position = new PointLatLng(66.4169575018027, 94.25025752215694);
            gMapControl1.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter; 
            gMapControl1.CanDragMap = true; 
            gMapControl1.DragButton = MouseButtons.Left; 
            gMapControl1.ShowCenter = false; 
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.OnMarkerEnter += GMapControl1_OnMarkerEnter;
            gMapControl1.OnMarkerLeave += GMapControl1_OnMarkerLeave;
            gMapControl1.MouseDown += GMapControl1_MouseDown;
            gMapControl1.MouseUp += GMapControl1_MouseUp;

            GetDataFromDataBase();

            gMapControl1.Overlays.Add(GetOverlayMarkers(OMCollection, " GroupsMarkers"));

        }

        private void GMapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if(selectedMarker != null)
            {
                var lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                var lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;
                selectedMarker.Position = new PointLatLng(lat, lng);
                gMapControl1.MouseMove -= GMapControl1_MouseMove;
                UpdateDataBase((OwnMarker)selectedMarker);
            }
        }

        private void GMapControl1_OnMarkerLeave(GMapMarker item)
        {
            selectedMarker = null;
        }

        private void GMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (gMapControl1.IsMouseOverMarker)
            {
                gMapControl1.MouseMove += GMapControl1_MouseMove;
            }
        }

        private void GMapControl1_OnMarkerEnter(GMapMarker item)
        {
            selectedMarker = item;
        }

        private void GMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            var lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
            var lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;
            selectedMarker.Position = new PointLatLng(lat, lng);
        }
        private GMapOverlay GetOverlayMarkers(List<OwnMarker> collection, string name)
        {
            GMapOverlay gMapMarkers = new GMapOverlay(name);
            foreach (OwnMarker mark in collection)
            {
                gMapMarkers.Markers.Add(mark);
            }
            return gMapMarkers;
        }

        private void GetDataFromDataBase()
        {
            using(SqlConnection db = new SqlConnection(connectionString))
            {
                db.Open();
                string sqlExpression = "Select * from vehicle";
                SqlCommand command = new SqlCommand(sqlExpression, db);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        OMCollection.Add(new OwnMarker(reader.GetGuid(0), reader.GetString(3), new PointLatLng(reader.GetDouble(1), reader.GetDouble(2)), GMarkerGoogleType.red));
                    }
                }
            }
        }

        private void UpdateDataBase(OwnMarker item)
        {
            using(SqlConnection db = new SqlConnection(connectionString))
            {
                db.Open();
                string sqlExpression = "Update vehicle set lat=@lat,lng=@lng where id=@id";
                SqlCommand command = new SqlCommand(sqlExpression, db);
                command.Parameters.Add(new SqlParameter("@lat", item.Position.Lat));
                command.Parameters.Add(new SqlParameter("@lng", item.Position.Lng));
                command.Parameters.Add(new SqlParameter("@id", item.id));
                command.ExecuteNonQuery();
                db.Close();
            }
        }
      
    }



    public class OwnMarker : GMarkerGoogle
    {
        public OwnMarker(Guid id,string text,PointLatLng pl,GMarkerGoogleType gmt) : base(pl,gmt)
        {
            this.id = id;
            ToolTip = new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(this);
            ToolTipText = text;
            ToolTipMode = MarkerTooltipMode.OnMouseOver;
        }
        public Guid id;
    }
}
