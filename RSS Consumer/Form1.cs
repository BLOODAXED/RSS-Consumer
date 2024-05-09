using Microsoft.Data.Sqlite;
using System.Data;
using System.Security.Policy;
using System.ServiceModel.Syndication;
using System.Xml;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.LinkLabel;


namespace RSS_Consumer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var connection = new SqliteConnection("Data Source=consumer.sqlite;Mode=ReadWriteCreate"))
            {
                var createTable = connection.CreateCommand();
                createTable.CommandText =
                    @"CREATE TABLE IF NOT EXISTS URLS(
name TEXT,
url TEXT
);
CREATE TABLE IF NOT EXISTS FEEDITEMS(
feed TEXT,
title TEXT,
link TEXT,
description TEXT
)";
                connection.Open();
                createTable.ExecuteNonQuery();
                updateFeedsList(connection);
            }
        }

        private string selectedFeed()
        {
            var feedName = listBox1.SelectedItem.ToString();
            if (String.IsNullOrEmpty(feedName))
            {
                MessageBox.Show("Select a Feed");
                return "";
            }
            return feedName;
        }

        private string getFeedURL(string feedName)
        {
            using (var connection = new SqliteConnection("Data Source=consumer.sqlite;Mode=ReadOnly"))
            {
                connection.Open();
                var url = connection.CreateCommand();
                url.CommandText = $"SELECT `url` FROM URLS Where `name` IS '{feedName}'";
                using (var reader = url.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }
            return "";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var feedName = selectedFeed();
            if (String.IsNullOrEmpty(feedName)) { return; }
            var url = getFeedURL(feedName);
            if (String.IsNullOrEmpty(url)) { return; }
            addFeedData(url, feedName);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFeed(textBox1.Text, textBox2.Text);
        }

        public void saveFeed(string url, string name)
        {
            if (name.Length == 0)
            {
                name = url;
            }

            using (var connection = new SqliteConnection("Data Source=consumer.sqlite;Mode=ReadWrite"))
            {
                connection.Open();
                var addFeed = connection.CreateCommand();
                addFeed.CommandText = $"INSERT INTO URLS(name, url) VALUES('{name}','{url}')";
                if (addFeed.ExecuteNonQuery() > 0)
                {
                    listBox1.Items.Add(name);
                }
                else
                {
                    MessageBox.Show($"Failed to add {name}");
                }

                updateFeedsList(connection);
            }
        }

        public void updateFeedsList(SqliteConnection connection)
        {
            listBox1.BeginUpdate();

            var getFeedsList = connection.CreateCommand();
            getFeedsList.CommandText = "Select `name` from URLS ORDER BY `name` ASC";

            listBox1.Items.Clear();

            using (var reader = getFeedsList.ExecuteReader())
            {

                while (reader.Read())
                {
                    listBox1.Items.Add(reader.GetString(0));
                }

            }

            listBox1.EndUpdate();
        }

        public void updateFeedItemsList(DataSet items)
        {
            feedData.Clear();

            foreach (DataRow row in items.Tables[0].Rows)
            {
                feedData.Text += row["title"].ToString() + "\n";
                feedData.Text += row["link"].ToString()+"\n";
                feedData.Text += row["description"].ToString() + "\n\n\n";
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {



        }

        private void addFeedData(string feedUrl, string feedName)
        {
            XmlReader reader = XmlReader.Create(feedUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            using (var connection = new SqliteConnection("Data Source=consumer.sqlite;Mode=ReadWrite"))
            {
                var itemCount = 0;
                connection.Open();
                var saveFeedItems = connection.CreateCommand();
                //look at feed items
                foreach (var item in feed.Items)
                {
                    itemCount++;
                    
                    var link = item.Links[0].Uri.ToString();

                    saveFeedItems.CommandText = $"" +
                        $"INSERT INTO FEEDITEMS (`feed`,`title`,`link`" +
                        $",`description`) SELECT '{feedName}', '{item.Title.Text}', " +
                        $"'{link}', '{item.Summary.Text}'" +
                        $"WHERE NOT EXISTS (SELECT `link` from FEEDITEMS where `link` IS '{link}')";

                    saveFeedItems.ExecuteNonQuery();
                }
                MessageBox.Show($"Cached {itemCount} items");
            }
        }

        private void viewFeedData(string feedName)
        {

            if (String.IsNullOrEmpty(feedName))
            {
                MessageBox.Show("No Feed Selected");

                return;
            }



        }

        private void feedData_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var feedName = selectedFeed();
            if (String.IsNullOrEmpty(feedName)) { return; }
            var url = getFeedURL(feedName);
            if (String.IsNullOrEmpty(url)) { return; }
            
            DataSet ds = new DataSet();
            using (SqliteCommand cmd = new SqliteCommand(
                $"SELECT * from FEEDITEMS where `feed` IS '{feedName}'",
                new SqliteConnection("Data Source=consumer.sqlite;Mode=ReadWrite")
                ))
            {
                cmd.Connection.Open();
                DataTable table = new DataTable();
                table.Load(cmd.ExecuteReader());
                ds.Tables.Add(table);
            }

            updateFeedItemsList(ds);

            /*DataTable entries = new DataTable();
            entries.Columns.Add("id");
            entries.Columns.Add("feed");
            entries.Columns.Add("table");
            entries.Columns.Add("link");
            entries.Columns.Add("description");
            using (var connection = new SqliteConnection("Data Source=consumer.sqlite;Mode=ReadWrite"))
            {
                connection.Open();
                var feedItems = connection.CreateCommand();
                feedItems.CommandText = $"SELECT `link` from FEEDITEMS where `feed` IS '{feedName}')";

                using (var items = feedItems.ExecuteReader())
                {
                    int rowNum = 0;
                    while (items.Read())
                    {
                        DataRow row = entries.NewRow();
                        row[""];
                        entries.Rows.Add(row);

                        entries.Rows[rowNum++] = row;
                    }
                }
            }*/

        }

        private void feedData_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
