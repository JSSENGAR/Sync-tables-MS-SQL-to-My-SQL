using MySql.Data.MySqlClient;

protected void websync()
        {
            toolStripComboBox2.Items.Clear();
            string connectionString = cc.mysqlcs();
            string SqlConnStr = cc.sqlcs();

            SqlConnection con = new SqlConnection("Data Source=" + textBox3.Text + ";Initial Catalog=SCP;User ID=" + textBox1.Text + ";Password=" + textBox2.Text);
            con.Close();
            con.Open();           
            
            string uidt1 = cc.C_Name("AutoWebSync");// sdrt1["FIELD"].ToString();
            
            if (uidt1 == "ON")
            {           

            SqlCommand cmdt = new SqlCommand("select TableName, KeyField from TableMst where MEnb='1' order by Id", con);
            SqlDataAdapter sdat = new SqlDataAdapter(cmdt);
            DataTable dt = new DataTable();
            sdat.Fill(dt);
            dataGridView2.DataSource = dt;                
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                    string selq = "";                  
                                   
                    selq = "select * from " + dataGridView2.Rows[i].Cells[0].Value + " where Sync = 1";
                    SqlCommand selcmd = new SqlCommand(selq, con);
                    SqlDataAdapter sdad = new SqlDataAdapter(selcmd);
                    DataTable dts = new DataTable();
                    sdad.Fill(dts);
                    dataGridView3.DataSource = dts;

                    toolStripLabel8.Text = dataGridView2.Rows[i].Cells[0].Value.ToString();
                    toolStripComboBox2.Items.Add(dataGridView2.Rows[i].Cells[0].Value.ToString());

                    DataSet SqldSet = new DataSet(); // SqlServer Dataset that holds Sql Server data
                    DataSet MySqldSet = new DataSet(); //MySql dataset that will be used to push data into MySql database


                    using (SqlDataAdapter dAd = new SqlDataAdapter(selq, con))
                        {
                            dAd.Fill(SqldSet, dataGridView2.Rows[i].Cells[0].Value.ToString());
                        }

                    MySqlTransaction mysqltr = null;
                    SqlTransaction sqltr = null;

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();                        
                        try
                        {
                            mysqltr = conn.BeginTransaction();
                            sqltr = con.BeginTransaction();
                            for (int j = 0; j < dataGridView3.Rows.Count; j++)
                            {
                                string qsd = "";
                                qsd = "delete from " + dataGridView2.Rows[i].Cells[0].Value + " where " + dataGridView2.Rows[i].Cells[1].Value + "='" + dataGridView3.Rows[j].Cells[dataGridView2.Rows[i].Cells[1].Value.ToString()].Value + "'";                                
                                using (MySqlCommand cmdmd = new MySqlCommand(qsd, conn, mysqltr))
                                {
                                    cmdmd.ExecuteNonQuery();
                                }                                                              
                            }
                            using (MySqlDataAdapter dAd = new MySqlDataAdapter(selq, conn))
                            {
                                dAd.Fill(MySqldSet, dataGridView2.Rows[i].Cells[0].Value.ToString()); // Got the empty table of MySql
                                                                                                  // Loop through all rows of Sql server data table and add into MySql dataset
                                foreach (DataRow row in SqldSet.Tables[dataGridView2.Rows[i].Cells[0].Value.ToString()].Rows)
                                {
                                    MySqldSet.Tables[0].NewRow();
                                    MySqldSet.Tables[0].Rows.Add(row.ItemArray);
                                }
                                // Now we have all rows of Sql Server into MySql server dataset
                                // Create a command builder to update MySql dataset 
                                MySqlCommandBuilder cmd = new MySqlCommandBuilder(dAd);
                                // Following update command will push all added rows into MySql dataset to database
                                dAd.Update(MySqldSet, dataGridView2.Rows[i].Cells[0].Value.ToString()); // We are done !!!
                            }
                            for (int j = 0; j < dataGridView3.Rows.Count; j++)
                            {
                                string qsu = "";
                                qsu = "UPDATE " + dataGridView2.Rows[i].Cells[0].Value + " set Sync=NULL where " + dataGridView2.Rows[i].Cells[1].Value + "='" + dataGridView3.Rows[j].Cells[dataGridView2.Rows[i].Cells[1].Value.ToString()].Value + "'";                                                                
                                
                                using (SqlCommand cmdmd = new SqlCommand(qsu, con,sqltr))
                                {
                                    cmdmd.ExecuteNonQuery();
                                }
                                using (MySqlCommand cmdmd = new MySqlCommand(qsu, conn, mysqltr))
                                {
                                    cmdmd.ExecuteNonQuery();
                                }
                        }
                        mysqltr.Commit();
                        sqltr.Commit();
                        }
                        catch //(Exception ee)
                        {
                            //statusStrip1.Text += ", " + ee.Message.ToString();
                            //MessageBox.Show(ee.Message.ToString());
                            conn.Open();
                            mysqltr.Rollback();
                            sqltr.Rollback();
                        }
                            conn.Close();
                      }
                
            }
            }
                con.Close();
        }