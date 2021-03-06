﻿using PortChatBot.Dialogs;
using PortChatBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace PortChatBot.DB
{
    public class DbConnect
    {
        static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/");
        const string CONSTRINGNAME = "conString";
        //ConnectionStringSettings connStr = rootWebConfig.ConnectionStrings.ConnectionStrings[CONSTRINGNAME]
        string connStr = rootWebConfig.ConnectionStrings.ConnectionStrings[CONSTRINGNAME].ToString();
        //string connStr = "Data Source=taiholab.database.windows.net;Initial Catalog=taihoLab_2;User ID=taihoinst;Password=taiho9788!;";
        //string connStr = "Data Source=10.6.222.21,1433;Initial Catalog=konadb;User ID=konadb;Password=Didwoehd20-9!;";
        //StringBuilder sb = new StringBuilder();
        public readonly string TEXTDLG = "2";
        public readonly string CARDDLG = "3";
        public readonly string MEDIADLG = "4";

        public void ConnectDb()
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

        }


        public List<DialogList> SelectInitDialog(String channel)
        {
            SqlDataReader rdr = null;
            List<DialogList> dialogs = new List<DialogList>();
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT   				    ";
                cmd.CommandText += " 	DLG_ID,                 ";
                cmd.CommandText += " 	DLG_TYPE,               ";
                cmd.CommandText += " 	DLG_GROUP,              ";
                cmd.CommandText += " 	DLG_ORDER_NO            ";
                cmd.CommandText += " FROM TBL_DLG     ";
                cmd.CommandText += " WHERE DLG_GROUP = '1'      ";
                cmd.CommandText += " AND USE_YN = 'Y'           ";
                cmd.CommandText += " ORDER BY DLG_ID            ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                DButil.HistoryLog(" db SelectInitDialog !! ");
                
                                
                while (rdr.Read())
                {
                    DialogList dlg = new DialogList();
                    dlg.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    dlg.dlgType = rdr["DLG_TYPE"] as string;
                    dlg.dlgGroup = rdr["DLG_GROUP"] as string;
                    dlg.dlgOrderNo = rdr["DLG_ORDER_NO"] as string;
                    
                    using (SqlConnection conn2 = new SqlConnection(connStr))
                    {
                        SqlCommand cmd2 = new SqlCommand();
                        conn2.Open();
                        cmd2.Connection = conn2;
                        SqlDataReader rdr2 = null;
                        if (dlg.dlgType.Equals(TEXTDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                            }
                            rdr2.Close();
                        } else if (dlg.dlgType.Equals(CARDDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, IMG_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT, " +
                                    "CARD_DIVISION, CARD_VALUE " +
                                    "FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' ";
                            //if (channel.Equals("facebook"))
                            //{
                            //    cmd2.CommandText += "FB_USE_YN = 'Y' ";
                            //}
                            cmd2.CommandText += "ORDER BY CARD_ORDER_NO";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);
                            List<CardList> dialogCards = new List<CardList>();
                            while (rdr2.Read())
                            {
                                CardList dlgCard = new CardList();
                                dlgCard.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlgCard.cardSubTitle = rdr2["CARD_SUBTITLE"] as string;
                                dlgCard.cardText = rdr2["CARD_TEXT"] as string;
                                dlgCard.imgUrl = rdr2["IMG_URL"] as string;
                                dlgCard.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlgCard.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlgCard.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlgCard.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlgCard.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlgCard.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlgCard.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlgCard.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlgCard.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlgCard.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlgCard.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlgCard.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                                dlgCard.cardDivision = rdr2["CARD_DIVISION"] as string;
                                dlgCard.cardValue = rdr2["CARD_VALUE"] as string;
                                dialogCards.Add(dlgCard);
                            }
                            dlg.dialogCard = dialogCards;
                        } else if (dlg.dlgType.Equals(MEDIADLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT, MEDIA_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT " +
                                    "FROM TBL_DLG_MEDIA WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                                dlg.mediaUrl = rdr2["MEDIA_URL"] as string;
                                dlg.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlg.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlg.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlg.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlg.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlg.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlg.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlg.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlg.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlg.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlg.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlg.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                            }
                        }
                        
                    }
                    dialogs.Add(dlg);
                }
                rdr.Close();
            }
            return dialogs;
        }

        public DialogList SelectDialog(int dlgID)
        {
            SqlDataReader rdr = null;
            DialogList dlg = new DialogList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT   				    ";
                cmd.CommandText += " 	DLG_ID,                 ";
                cmd.CommandText += " 	DLG_NAME,               ";
                cmd.CommandText += " 	DLG_DESCRIPTION,        ";
                cmd.CommandText += " 	DLG_LANG,               ";
                cmd.CommandText += " 	DLG_TYPE,               ";
                cmd.CommandText += " 	DLG_ORDER_NO,           ";
                cmd.CommandText += " 	DLG_GROUP               ";
                cmd.CommandText += " FROM TBL_DLG               ";
                cmd.CommandText += " WHERE DLG_ID = @dlgId      ";
                cmd.CommandText += " AND USE_YN = 'Y'           ";
                cmd.CommandText += " ORDER BY  DLG_ORDER_NO     ";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    dlg.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    dlg.dlgType = rdr["DLG_TYPE"] as string;
                    dlg.dlgGroup = rdr["DLG_GROUP"] as string;
                    dlg.dlgOrderNo = rdr["DLG_ORDER_NO"] as string;

                    using (SqlConnection conn2 = new SqlConnection(connStr))
                    {
                        SqlCommand cmd2 = new SqlCommand();
                        conn2.Open();
                        cmd2.Connection = conn2;
                        SqlDataReader rdr2 = null;
                        if (dlg.dlgType.Equals(TEXTDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                            }
                            rdr2.Close();
                        }
                        else if (dlg.dlgType.Equals(CARDDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, IMG_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT, " +
                                    "CARD_DIVISION, CARD_VALUE, CARD_ORDER_NO " +
                                    "FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' ORDER BY CARD_ORDER_NO";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);
                            List<CardList> dialogCards = new List<CardList>();
                            while (rdr2.Read())
                            {
                                CardList dlgCard = new CardList();
                                dlgCard.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlgCard.cardSubTitle = rdr2["CARD_SUBTITLE"] as string;
                                dlgCard.cardText = rdr2["CARD_TEXT"] as string;
                                dlgCard.imgUrl = rdr2["IMG_URL"] as string;
                                dlgCard.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlgCard.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlgCard.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlgCard.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlgCard.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlgCard.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlgCard.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlgCard.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlgCard.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlgCard.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlgCard.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlgCard.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                                dlgCard.cardDivision = rdr2["CARD_DIVISION"] as string;
                                dlgCard.cardValue = rdr2["CARD_VALUE"] as string;
                                //dlgCard.card_order_no = rdr2["CARD_ORDER_NO"] as string;
                                dlgCard.card_order_no = Convert.ToInt32(rdr2["CARD_ORDER_NO"]);

                                dialogCards.Add(dlgCard);
                            }
                            dlg.dialogCard = dialogCards;
                        }
                        else if (dlg.dlgType.Equals(MEDIADLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT, MEDIA_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT , CARD_DIVISION, CARD_VALUE " +
                                    "FROM TBL_DLG_MEDIA WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                                dlg.mediaUrl = rdr2["MEDIA_URL"] as string;
                                dlg.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlg.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlg.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlg.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlg.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlg.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlg.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlg.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlg.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlg.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlg.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlg.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                                dlg.cardDivision = rdr2["CARD_DIVISION"] as string;
                                dlg.cardValue = rdr2["CARD_VALUE"] as string;
                            }
                        }

                    }
                }
            }
            return dlg;
        }

        public List<CardList> SelectDialogCard(int dlgID)
        {
            SqlDataReader rdr = null;
            List<CardList> dialogCard = new List<CardList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT CARD_DLG_ID, DLG_ID, CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, IMG_URL," +
                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, " +
                    "CARD_DIVISION, CARD_VALUE " +
                    "FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999 ORDER BY CARD_ORDER_NO";
                    //"FROM TBL_SECCS_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999 ORDER BY CARD_ORDER_NO";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int cardDlgId = Convert.ToInt32(rdr["CARD_DLG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardSubTitle = rdr["CARD_SUBTITLE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;
                    string imgUrl = rdr["IMG_URL"] as string;
                    string btn1Type = rdr["BTN_1_TYPE"] as string;
                    string btn1Title = rdr["BTN_1_TITLE"] as string;
                    string btn1Context = rdr["BTN_1_CONTEXT"] as string;
                    string btn2Type = rdr["BTN_2_TYPE"] as string;
                    string btn2Title = rdr["BTN_2_TITLE"] as string;
                    string btn2Context = rdr["BTN_2_CONTEXT"] as string;
                    string btn3Type = rdr["BTN_3_TYPE"] as string;
                    string btn3Title = rdr["BTN_3_TITLE"] as string;
                    string btn3Context = rdr["BTN_3_CONTEXT"] as string;
                    string cardDivision = rdr["CARD_DIVISION"] as string;
                    string cardValue = rdr["CARD_VALUE"] as string;

                    CardList dlgCard = new CardList();
                    dlgCard.cardDlgId = cardDlgId;
                    dlgCard.dlgId = dlgId;
                    dlgCard.cardTitle = cardTitle;
                    dlgCard.cardSubTitle = cardSubTitle;
                    dlgCard.cardText = cardText;
                    dlgCard.imgUrl = imgUrl;
                    dlgCard.btn1Type = btn1Type;
                    dlgCard.btn1Title = btn1Title;
                    dlgCard.btn1Context = btn1Context;
                    dlgCard.btn2Type = btn2Type;
                    dlgCard.btn2Title = btn2Title;
                    dlgCard.btn2Context = btn2Context;
                    dlgCard.btn3Type = btn3Type;
                    dlgCard.btn3Title = btn3Title;
                    dlgCard.btn3Context = btn3Context;
                    dlgCard.cardDivision = cardDivision;
                    dlgCard.cardValue = cardValue;

                    dialogCard.Add(dlgCard);
                }
            }
            return dialogCard;
        }

        public List<TextList> SelectDialogText(int dlgID)
        {
            SqlDataReader rdr = null;
            List<TextList> dialogText = new List<TextList>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE, CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";
                //cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE, CARD_TEXT FROM TBL_SECCS_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int textDlgId = Convert.ToInt32(rdr["TEXT_DLG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;


                    TextList dlgText = new TextList();
                    dlgText.textDlgId = textDlgId;
                    dlgText.dlgId = dlgId;
                    dlgText.cardTitle = cardTitle;
                    dlgText.cardText = cardText;


                    dialogText.Add(dlgText);
                }
            }
            return dialogText;
        }


        public List<TextList> SelectSorryDialogText(string dlgGroup)
        {
            SqlDataReader rdr = null;
            List<TextList> dialogText = new List<TextList>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE,CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = (SELECT DLG_ID FROM TBL_DLG WHERE DLG_GROUP = @dlgGroup) AND USE_YN = 'Y'";
                //cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE, CARD_TEXT FROM TBL_SECCS_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgGroup", dlgGroup);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int textDlgId = Convert.ToInt32(rdr["TEXT_DLG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;
                    
                    TextList dlgText = new TextList();
                    dlgText.textDlgId = textDlgId;
                    dlgText.dlgId = dlgId;
                    dlgText.cardTitle = cardTitle;
                    dlgText.cardText = cardText;
                    
                    dialogText.Add(dlgText);
                }
            }
            return dialogText;
        }
        
        
        //KSO START
        public CardList BannedChk(string orgMent)
        {
            SqlDataReader rdr = null;
            CardList SelectBanned = new CardList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT                                                                                                                                                         ";
                cmd.CommandText += " TOP 1 TD.DLG_ID, (SELECT TOP 1 BANNED_WORD FROM TBL_BANNED_WORD_LIST WHERE CHARINDEX(BANNED_WORD, @msg) > 0) AS BANNED_WORD, TDT.CARD_TITLE, TDT.CARD_TEXT     ";
                cmd.CommandText += " FROM TBL_DLG TD, TBL_DLG_TEXT TDT                                                                                                                              ";
                cmd.CommandText += " WHERE TD.DLG_ID = TDT.DLG_ID                                                                                                                                   ";
                cmd.CommandText += " AND                                                                                                                                                            ";
                cmd.CommandText += " 	TD.DLG_GROUP =                                                                                                                                              ";
                cmd.CommandText += " 	(                                                                                                                                                           ";
                cmd.CommandText += " 	   SELECT CASE WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 3 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 3                                                  ";
                cmd.CommandText += " 			  WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 4 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 4                                                       ";
                cmd.CommandText += " 			 END                                                                                                                                                ";
                cmd.CommandText += " 	   FROM TBL_BANNED_WORD_LIST A                                                                                                                              ";
                cmd.CommandText += " 	) AND TD.DLG_GROUP IN (3,4)                                                                                                                                 ";
                cmd.CommandText += " ORDER BY NEWID()                                                                                                                                               ";

                cmd.Parameters.AddWithValue("@msg", orgMent);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    //answerMsg = rdr["CARD_TEXT"] + "@@" + rdr["DLG_ID"] + "@@" + rdr["CARD_TITLE"];

                    int dlg_id = Convert.ToInt32(rdr["DLG_ID"]);
                    String card_title = rdr["CARD_TITLE"] as String;
                    String card_text = rdr["CARD_TEXT"] as String;
                    
                    SelectBanned.dlgId = dlg_id;
                    SelectBanned.cardTitle = card_title;
                    SelectBanned.cardText = card_text;
                }
            }
            return SelectBanned;
        }

        public CacheList CacheChk(string orgMent)
        {
            SqlDataReader rdr = null;
            CacheList result = new CacheList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, LUIS_ENTITIES_VALUE, ISNULL(LUIS_INTENT_SCORE,'') AS LUIS_INTENT_SCORE FROM TBL_QUERY_ANALYSIS_RESULT WHERE LOWER(QUERY) = LOWER(@msg) AND RESULT ='H'";

                cmd.Parameters.AddWithValue("@msg", orgMent);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                
                /*
                if (rdr.Read())
                {
                    Debug.WriteLine("* YES - TBL_QUERY_ANALYSIS_RESULT");
                }
                else
                {
                    Debug.WriteLine("* NO - TBL_QUERY_ANALYSIS_RESULT");
                }
                */

                while (rdr.Read())
                {
                    string luisId = rdr["LUIS_ID"] as String;
                    string intentId = rdr["LUIS_INTENT"] as String;
                    string entitiesId = rdr["LUIS_ENTITIES"] as String;
                    string entitiesValue= rdr["LUIS_ENTITIES_VALUE"] as String;
                    string luisScore = rdr["LUIS_INTENT_SCORE"] as String;
                    
                    result.luisId = luisId;
                    result.luisIntent = intentId;
                    result.luisEntities = entitiesId;
                    result.luisEntitiesValue = entitiesValue;
                    result.luisScore = luisScore;

                    Debug.WriteLine("Yes rdr | intentId : " + intentId + " | entitiesId : "+ entitiesId + " | entitiesValue : " + entitiesValue + " | luisScore : " + luisScore);
                }

            }
            return result;
        }

        public CacheList CacheDataFromIntent(string intent, String luisEntitiesValue)
        {
            SqlDataReader rdr = null;
            CacheList result = new CacheList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, '' AS LUIS_INTENT_SCORE FROM TBL_DLG_RELATION_LUIS WHERE LUIS_INTENT=@intent";

                cmd.Parameters.AddWithValue("@intent", intent);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                /*
                if (rdr.Read())
                {
                    Debug.WriteLine("* YES - TBL_QUERY_ANALYSIS_RESULT");
                }
                else
                {
                    Debug.WriteLine("* NO - TBL_QUERY_ANALYSIS_RESULT");
                }
                */

                while (rdr.Read())
                {
                    string luisId = rdr["LUIS_ID"] as String;
                    string intentId = rdr["LUIS_INTENT"] as String;
                    string entitiesId = rdr["LUIS_ENTITIES"] as String;
                    string luisScore = rdr["LUIS_INTENT_SCORE"] as String;
                    //string luisEntitiesValue = "" as String;

                    result.luisId = luisId;
                    result.luisIntent = intentId;
                    result.luisEntities = entitiesId;
                    result.luisScore = luisScore;
                    result.luisEntitiesValue = luisEntitiesValue;


                    Debug.WriteLine("Yes rdr | intentId : " + intentId + " | entitiesId : " + entitiesId + " | luisScore : " + luisScore);
                }

            }
            return result;
        }

        public QueryIntentList SelectQueryIntent(string orgMent)
        {
            SqlDataReader rdr = null;
            QueryIntentList result = new QueryIntentList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, ISNULL(LUIS_INTENT_SCORE,'') AS LUIS_INTENT_SCORE FROM TBL_QUERY_ANALYSIS_RESULT WHERE LOWER(QUERY) = LOWER(@msg) AND RESULT ='H'";
                cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, DLG_ID FROM TBL_QUERY_INTENT WHERE LOWER(QUERY) = LOWER(@msg) ";

                cmd.Parameters.AddWithValue("@msg", orgMent);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                
                while (rdr.Read())
                {
                    string luisId = rdr["LUIS_ID"] as String;
                    string intentId = rdr["LUIS_INTENT"] as String;
                    //string dlgid = rdr["DLG_ID"] as String;
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"] ?? 0);

                    result.luisId = luisId;
                    result.luisIntent = intentId;
                    result.dlgId = dlgId;
                    Debug.WriteLine("Yes rdr | intentId : " + intentId + " | intentId : " + intentId + " | dlgid : " + dlgId);
                }

            }
            return result;
        }



        public List<RelationList> DefineTypeChk(string luisId, string intentId, string entitiesId)
        {
            SqlDataReader rdr = null;
            List<RelationList> result = new List<RelationList>();
            Debug.WriteLine("luisId ::: "+ luisId);
            Debug.WriteLine("intentId ::: " + intentId);
            Debug.WriteLine("entitiesId ::: " + entitiesId);
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, ISNULL(DLG_ID,0) AS DLG_ID, DLG_API_DEFINE, API_ID ";
                cmd.CommandText += "  FROM TBL_DLG_RELATION_LUIS                                                    ";
                //cmd.CommandText += " WHERE 1=1                                               ";
                cmd.CommandText += " WHERE LUIS_INTENT = @intentId                                                 ";
                cmd.CommandText += "   AND LUIS_ENTITIES = @entities                                                ";
                //cmd.CommandText += "   AND LUIS_ID = @luisId                                                        ";

                if(intentId != null){
                    cmd.Parameters.AddWithValue("@intentId", intentId);
                }else{
                    cmd.Parameters.AddWithValue("@intentId", DBNull.Value);
                }

                if (entitiesId != null){
                    cmd.Parameters.AddWithValue("@entities", entitiesId);
                }else{
                    cmd.Parameters.AddWithValue("@entities", DBNull.Value);
                }

                if (luisId != null){
                    cmd.Parameters.AddWithValue("@luisId", luisId);
                }
                else{
                    cmd.Parameters.AddWithValue("@luisId", DBNull.Value);
                }


                

                Debug.WriteLine("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    RelationList relationList = new RelationList();
                    relationList.luisId = rdr["LUIS_ID"] as string;
                    relationList.luisIntent = rdr["LUIS_INTENT"] as string;
                    relationList.luisEntities = rdr["LUIS_ENTITIES"] as string;
                    relationList.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    relationList.dlgApiDefine = rdr["DLG_API_DEFINE"] as string;
                    //relationList.apiId = Convert.ToInt32(rdr["API_ID"] ?? 0);
                    relationList.apiId = rdr["API_ID"].Equals(DBNull.Value)? 0 : Convert.ToInt32(rdr["API_ID"]) ;
                    //DBNull.Value
                    result.Add(relationList);
                }
            }
            return result;
        }

        public List<RelationList> DefineTypeChkSpare(string entity)
        {
            SqlDataReader rdr = null;
            List<RelationList> result = new List<RelationList>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT  LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, ISNULL(DLG_ID,0) AS DLG_ID, DLG_API_DEFINE, API_ID ";
                cmd.CommandText += "  FROM  TBL_DLG_RELATION_LUIS                                                    ";
                //cmd.CommandText += " WHERE  LUIS_ENTITIES = @entities                                                ";
                cmd.CommandText += " WHERE  LUIS_ENTITIES = @intentId                                                ";
                

                Debug.WriteLine("query : " + cmd.CommandText);
                cmd.Parameters.AddWithValue("@intentId", entity);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    RelationList relationList = new RelationList();
                    relationList.luisId = rdr["LUIS_ID"] as string;
                    relationList.luisIntent = rdr["LUIS_INTENT"] as string;
                    relationList.luisEntities = rdr["LUIS_ENTITIES"] as string;
                    relationList.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    relationList.dlgApiDefine = rdr["DLG_API_DEFINE"] as string;
                    //relationList.apiId = Convert.ToInt32(rdr["API_ID"] ?? 0);
                    relationList.apiId = rdr["API_ID"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(rdr["API_ID"]);
                    //DBNull.Value
                    result.Add(relationList);
                }
            }
            return result;
        }


        //KSO END

        //TBL_CHATBOT_CONF 정보 가져오기
        //      LUIS_APP_ID	    - 루이스APP_ID
        //      LUIS_TIME_LIMIT - 루이스제한
        //      LUIS_SCORE_LIMIT - 스코어 제한
        //      LUIS_SUBSCRIPTION   - 루이스구독
        //      BOT_NAME        - 봇이름?
        //      BOT_APP_ID      - 봇앱아이디?
        //      BOT_APP_PASSWORD- 봇앱패스워드?
        //      QUOTE           - 견적url
        //      TESTDRIVE       - 시승url
        //      CATALOG         - 카달로그url
        //      DISCOUNT        - 할인url
        //      EVENT           - 이벤트url

        public List<ConfList> SelectConfig()
        //public List<ConfList> SelectConfig(string config_type)
        {
            SqlDataReader rdr = null;
            List<ConfList> conflist = new List<ConfList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                DButil.HistoryLog("db conn SelectConfig !!");
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = " SELECT CNF_TYPE, CNF_NM, CNF_VALUE" +
                                  " FROM TBL_CHATBOT_CONF " +
                                  //" WHERE CNF_TYPE = 'LUIS_APP_ID' " +
                                  " ORDER BY CNF_TYPE DESC, ORDER_NO ASC ";

                Debug.WriteLine("* cmd.CommandText : " + cmd.CommandText);
                //cmd.Parameters.AddWithValue("@config_type", config_type);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string cnfType = rdr["CNF_TYPE"] as string;
                    string cnfNm = rdr["CNF_NM"] as string;
                    string cnfValue = rdr["CNF_VALUE"] as string;

                    ConfList list = new ConfList();

                    list.cnfType = cnfType;
                    list.cnfNm = cnfNm;
                    list.cnfValue = cnfValue;


                    Debug.WriteLine("* cnfNm : " + cnfNm + " || cnfValue : " + cnfValue);
                    DButil.HistoryLog("* cnfNm : " + cnfNm + " || cnfValue : " + cnfValue);
                    conflist.Add(list);
                }
            }
            return conflist;
        }

		public string SelectChgMsg(string oldMsg)
		{
			SqlDataReader rdr = null;
			string newMsg = "";

			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;

				cmd.CommandText += "	SELECT FIND.CHG  CHG_WORD FROM(    					    ";
				cmd.CommandText += "	SELECT                                                  ";
				cmd.CommandText += "			CASE WHEN LEN(ORG_WORD) = LEN(@oldMsg)          ";
				cmd.CommandText += "				THEN CHARINDEX(ORG_WORD, @oldMsg)           ";
				cmd.CommandText += "				ELSE 0                                      ";
				cmd.CommandText += "				END AS FIND_NUM,                            ";
				cmd.CommandText += "				REPLACE(@oldMsg, ORG_WORD, CHG_WORD) CHG    ";
				cmd.CommandText += "	  FROM TBL_WORD_CHG_DICT                                ";
				cmd.CommandText += "	  ) FIND                                                ";
				cmd.CommandText += "	  WHERE FIND.FIND_NUM > 0                               ";





				cmd.Parameters.AddWithValue("@oldMsg", oldMsg);

				rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

				while (rdr.Read())
				{
					newMsg = rdr["CHG_WORD"] as string;
				}
			}
			return newMsg;
		}
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Query Analysis
		// Insert user chat message for history and analysis
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int insertUserQuery()
		{
			int dbResult = 0;
			using (SqlConnection conn = new SqlConnection(connStr))
			{
                String luisID = "", intentName = "", entities = "", entitiesValue ="", result = "", intentScore = "";

                int appID = 0,luisScore = 0;


                //if(MessagesController.recommendResult != "recommend")
                if(MessagesController.apiFlag != "RECOMMEND")
                {
                    //if (MessagesController.relationList.Equals(null))
                    if (MessagesController.relationList == null)
                    {
                        entities = "None";
                        entitiesValue = "None";
                        intentName = "None";
                        luisID = "None";
                        luisScore = 0;
                    }
                    else
                    {

                        if (MessagesController.relationList.Count() > 0)
                        {
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisId))
                            {
                                luisID = "None";
                            }
                            else
                            {
                                luisID = MessagesController.relationList[0].luisId;
                            }
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisIntent))
                            {
                                intentName = "None";
                            }
                            else
                            {
                                intentName = MessagesController.relationList[0].luisIntent;
                            }
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisEntities))
                            {
                                entities = "None";
                            }
                            else
                            {
                                entities = MessagesController.relationList[0].luisEntities;
                            }
                            if (String.IsNullOrEmpty(MessagesController.luisEntitiesValue))
                            {
                                entitiesValue = "None";
                            }
                            else
                            {
                                entitiesValue = MessagesController.luisEntitiesValue;
                            }
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisScore.ToString()))
                            {
                                intentScore = "0";
                            }
                            else
                            {
                                intentScore = MessagesController.relationList[0].luisScore.ToString();
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisId))
                            {
                                luisID = "None";
                            }
                            else
                            {
                                luisID = MessagesController.cacheList.luisId;
                            }
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisIntent))
                            {
                                intentName = "None";
                            }
                            else
                            {
                                intentName = MessagesController.cacheList.luisIntent;
                            }
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisEntities))
                            {
                                entities = "None";
                            }
                            else
                            {
                                entities = MessagesController.cacheList.luisEntities;
                            }
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisScore))
                            {
                                intentScore = "0";
                            }
                            else
                            {
                                intentScore = MessagesController.cacheList.luisScore;
                            }
                        }
                    }

                    if (String.IsNullOrEmpty(MessagesController.replyresult))
                    {
                        result = "D";
                    }
                    else
                    {
                        result = MessagesController.replyresult;
                    }
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_insertusehistory4";

                    cmd.CommandType = CommandType.StoredProcedure;


                    //if (result.Equals("S") || result.Equals("D"))
                    //{
                    //    cmd.Parameters.AddWithValue("@Query", "");
                    //    cmd.Parameters.AddWithValue("@intentID", "");
                    //    cmd.Parameters.AddWithValue("@entitiesIDS", "");
                    //    cmd.Parameters.AddWithValue("@intentScore", "");
                    //    cmd.Parameters.AddWithValue("@luisID", "");
                    //    cmd.Parameters.AddWithValue("@result", result);
                    //    cmd.Parameters.AddWithValue("@appID", appID);
                    //}
                    //else
                    //{
                    Debug.WriteLine("DDDDDD : " + Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline).Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Query", Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline).Trim().ToLower());
                    cmd.Parameters.AddWithValue("@intentID", intentName.Trim());
                    cmd.Parameters.AddWithValue("@entitiesIDS", entities.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@entitiesIDSV", entitiesValue.Trim().ToLower());
                    
                    if (result.Equals("D") || result.Equals("S"))
                    {
                        cmd.Parameters.AddWithValue("@intentScore", "0");
                    }
                    else
                    {
                        //if(MessagesController.relationList != null)
                        //{
                            if (MessagesController.relationList.Count > 0 && MessagesController.relationList[0].luisEntities != null)
                            {
                                cmd.Parameters.AddWithValue("@intentScore", MessagesController.relationList[0].luisScore);
                            }
                        //}
                        else
                        {
                            cmd.Parameters.AddWithValue("@intentScore", MessagesController.cacheList.luisScore);
                        }
                    }
                    cmd.Parameters.AddWithValue("@luisID", luisID);
                    cmd.Parameters.AddWithValue("@result", result);
                    cmd.Parameters.AddWithValue("@appID", appID);
                    //}

                    dbResult = cmd.ExecuteNonQuery();
                }

                
			}
			return dbResult;
		}

        public int insertUserQuery(string korQuery, string intentID, string entitiesIDS, string intentScore, String luisID, char result, int appID)
        {
            int dbResult = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "sp_insertusehistory4";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Query", korQuery.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentID", intentID.Trim());
                cmd.Parameters.AddWithValue("@entitiesIDS", entitiesIDS.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentScore", intentScore.Trim().ToLower());
                cmd.Parameters.AddWithValue("@luisID", luisID);
                cmd.Parameters.AddWithValue("@result", result);
                cmd.Parameters.AddWithValue("@appID", appID);


                dbResult = cmd.ExecuteNonQuery();
            }
            return dbResult;
        }



        public int insertHistory(string userNumber, string channel, int responseTime)
		{
            //SqlDataReader rdr = null;
            int appID = 0;
            int result;
            String intentName = "";

            //if (MessagesController.relationList.Equals(null))
            if (MessagesController.relationList == null)
            {
                intentName = "None";
            }
            else
            {
                if (MessagesController.relationList.Count() > 0)
                {
                    if (String.IsNullOrEmpty(MessagesController.relationList[0].luisIntent))
                    {
                        intentName = "None";
                    }
                    else
                    {
                        intentName = MessagesController.relationList[0].luisIntent;
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(MessagesController.cacheList.luisIntent))
                    {
                        intentName = "None";
                    }
                    else
                    {
                        intentName = MessagesController.cacheList.luisIntent;
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText += " INSERT INTO TBL_HISTORY_QUERY ";
				cmd.CommandText += " (USER_NUMBER, CUSTOMER_COMMENT_KR, CHATBOT_COMMENT_CODE, CHANNEL, RESPONSE_TIME, REG_DATE, ACTIVE_FLAG, APP_ID) ";
				cmd.CommandText += " VALUES ";
				cmd.CommandText += " (@userNumber, @customerCommentKR, @chatbotCommentCode, @channel, @responseTime, CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  DATEADD( HH, 9, GETDATE() ), 24), 0, @appID) ";

				cmd.Parameters.AddWithValue("@userNumber", userNumber);
				cmd.Parameters.AddWithValue("@customerCommentKR", MessagesController.queryStr);

                if(MessagesController.replyresult.Equals("S"))
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", "SEARCH");
                }
                else if (MessagesController.replyresult.Equals("D"))
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", "ERROR");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", intentName);
                }
                    
				cmd.Parameters.AddWithValue("@channel", channel);
				cmd.Parameters.AddWithValue("@responseTime", responseTime);
				cmd.Parameters.AddWithValue("@appID", appID);

				result = cmd.ExecuteNonQuery();
				Debug.WriteLine("result : " + result);
			}
			return result;
		}

		public int SelectUserQueryErrorMessageCheck(string userID, int appID)
		{
			SqlDataReader rdr = null;
			int result = 0;
			//userID = arg.Replace("'", "''");
			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;

				cmd.CommandText += " SELECT TOP 1 A.CHATBOT_COMMENT_CODE ";
				cmd.CommandText += " FROM ( ";
				cmd.CommandText += " 	SELECT  ";
				cmd.CommandText += " 		SID, ";
				cmd.CommandText += " 		CASE  CHATBOT_COMMENT_CODE  ";
				cmd.CommandText += " 			WHEN 'SEARCH' THEN '1' ";
				cmd.CommandText += " 			WHEN 'ERROR' THEN '1' ";
				cmd.CommandText += " 			ELSE '0' ";
				cmd.CommandText += " 		END CHATBOT_COMMENT_CODE ";
				cmd.CommandText += " 	FROM TBL_HISTORY_QUERY WHERE USER_NUMBER = '" + userID + "' AND APP_ID = " + appID;
				cmd.CommandText += " ) A ";
				cmd.CommandText += " ORDER BY A.SID DESC ";

				rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

				while (rdr.Read())
				{
					result = Convert.ToInt32(rdr["CHATBOT_COMMENT_CODE"]);
				}
			}
			return result;
		}



        public string SelectArray(string entities)
        {
            SqlDataReader rdr = null;
            string newMsg = "";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "	SELECT ";
                cmd.CommandText += "        ISNULL(MAX(CASE WHEN POS = 1 THEN VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 2 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 3 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 4 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 5 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 6 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 7 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 8 THEN ',' + VAL1 END), '') AS VAL ";
                cmd.CommandText += "        FROM ";
                cmd.CommandText += "            ( ";
                cmd.CommandText += "                SELECT VAL1, POS ";
                cmd.CommandText += "                FROM Split2(@entities, ',') ";
                cmd.CommandText += "            ) A                             ";

                cmd.Parameters.AddWithValue("@entities", entities);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    newMsg = rdr["VAL"] as string;
                }
            }
            return newMsg;
        }

        public String SearchCommonEntities
        {
            get
            {
                String query = Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline).Replace(" ", "");
                SqlDataReader rdr = null;
                //List<RecommendConfirm> rc = new List<RecommendConfirm>();
                String entityarr = "";

                using (SqlConnection conn = new SqlConnection(connStr))
                {

                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    //cmd.CommandText += "SELECT  ENTITY_VALUE, ENTITY ";
                    //cmd.CommandText += "FROM    TBL_COMMON_ENTITY_DEFINE ";
                    //cmd.CommandText += "WHERE   CHARINDEX(ENTITY_VALUE,@kr_query) > 0";

                    cmd.CommandText += "SELECT RESULT AS ENTITIES FROM FN_ENTITY_ORDERBY_ADD(@kr_query) ";

                    cmd.Parameters.AddWithValue("@kr_query", query);
                    Debug.WriteLine("* SearchCommonEntities (CommandText : " + cmd.CommandText + ")");

                    rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    //int count = 0;
                    try
                    {
                        while (rdr.Read())
                        {
                            entityarr += rdr["ENTITIES"];
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                }
                return entityarr;
            }
        }

        public String SelectUserHistoryComment(string userNumber, string chatbotCommentCode)
        {
            // userNumber, chatbotCommentCode
            string resultComment = "";
            SqlDataReader rdr = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "	SELECT ";
                cmd.CommandText += "        TOP 1 ISNULL(CUSTOMER_COMMENT_KR, '') AS COMMENT ";
                cmd.CommandText += "        FROM TBL_HISTORY_QUERY";
                cmd.CommandText += " 	WHERE USER_NUMBER = '" + userNumber + "' AND CHATBOT_COMMENT_CODE = '" + chatbotCommentCode + "'";
                cmd.CommandText += "    ORDER BY SID DESC";
                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@chatbotCommentCode", chatbotCommentCode);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    resultComment = rdr["COMMENT"] as string;

                }

                //resultComment = newComment;
            }

            return resultComment;
        }

        public List<HrList> SelectHrInfo(string workerId)
        {
            SqlDataReader rdr = null;
            List<HrList> result = new List<HrList>();
            DButil.HistoryLog("* SelectHrInfo start: ");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                DButil.HistoryLog("* SelectHrInfo commandText start: ");
                cmd.CommandText += "	SELECT ";
                cmd.CommandText += "        EMP_NO, USER_NM, DEPT_CD, DEPT_NM, USER_ID ";
                cmd.CommandText += "        FROM ADM_USER";
                cmd.CommandText += " 	WHERE EMP_NO = '" + workerId + "' ";
                cmd.Parameters.AddWithValue("@workerId", workerId);

                Debug.WriteLine("* SelectHrInfo() CommandText : " + cmd.CommandText);
                DButil.HistoryLog("* SelectHrInfo : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    HrList hrList = new HrList();
                    hrList.emp_no = rdr["EMP_NO"] as string;
                    hrList.user_nm = rdr["USER_NM"] as string;
                    hrList.dept_cd = rdr["DEPT_CD"] as string;
                    hrList.dept_nm = rdr["DEPT_NM"] as string;
                    hrList.user_id = rdr["USER_ID"] as string;
                    result.Add(hrList);
                }

                return result;
            }

        }

        public List<AnalysisList> SelectAnalysisInfo(string tmnCod, string eqpTypNam)
        {
            SqlDataReader rdr = null;
            List<AnalysisList> result = new List<AnalysisList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "    SELECT ";
                cmd.CommandText += "        TMN_COD, EQP_TYP, EQP_TYP_NAME, ACCIDENTTYPE, FACTOR1, FACTOR2, FACTOR3, ANALYSIS ";
                cmd.CommandText += "        FROM PORT_ACCIDENT_ANALYSIS ";
                cmd.CommandText += " 	WHERE TMN_COD = '" + tmnCod + "' AND EQP_TYP_NAME = '" + eqpTypNam + "'";
                cmd.Parameters.AddWithValue("@tmnCod", tmnCod);
                cmd.Parameters.AddWithValue("@eqpTypNam", eqpTypNam);

                Debug.WriteLine("* SelectAnalysisInfo() CommandText : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    AnalysisList analysisList = new AnalysisList();
                    analysisList.tmn_cod = rdr["TMN_COD"] as string;
                    analysisList.eqp_typ = rdr["EQP_TYP"] as string;
                    analysisList.eqp_typ_name = rdr["EQP_TYP_NAME"] as string;
                    analysisList.accidenttype = rdr["ACCIDENTTYPE"] as string;
                    analysisList.factor1 = rdr["FACTOR1"] as string;
                    analysisList.factor2 = rdr["FACTOR2"] as string;
                    analysisList.factor3 = rdr["FACTOR3"] as string;
                    analysisList.analysis = rdr["ANALYSIS"] as string;
                    result.Add(analysisList);
                }

                return result;
            }
        }

         


        public List<TrendList> SelectTrendInfo(string eqpTyp)
        {
            SqlDataReader rdr = null;
            List<TrendList> result = new List<TrendList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                /*
                cmd.CommandText += "	SELECT ";
                cmd.CommandText += "        YEAR, MONTH, EQP_TYP, ACCIDENTTYPE, COUNT ";
                cmd.CommandText += "        FROM PORT_ACCIDENT_TREND ";
                cmd.CommandText += " 	WHERE EQP_TYP = '" + eqpTyp + "' ";
                cmd.Parameters.AddWithValue("@eqpTyp", eqpTyp);
                */
                //select ACCIDENTTYPE, SUM(COUNT) AS countSum from PORT_ACCIDENT_TREND group by ACCIDENTTYPE ORDER BY countSum DESC
                cmd.CommandText += "    SELECT ACCIDENTTYPE, SUM(COUNT) AS COUNTSUM ";
                cmd.CommandText += "        FROM PORT_ACCIDENT_TREND ";
                cmd.CommandText += "        GROUP BY ACCIDENTTYPE ORDER BY COUNTSUM DESC ";

                Debug.WriteLine("* SelectTrendInfo() CommandText : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    TrendList trendList = new TrendList();
                    //trendList.year = rdr["YEAR"] as string;
                    //trendList.month = rdr["MONTH"] as string;
                    //trendList.eqp_typ = rdr["EQP_TYP"] as string;
                    trendList.accidenttype = rdr["ACCIDENTTYPE"] as string;
                    trendList.count = Convert.ToInt32(rdr["COUNTSUM"]);
                    result.Add(trendList);
                }

                return result;

            }
        }

        public List<WeatherList> SelectWeatherInfo(string strTime)
        {
            SqlDataReader rdr = null;
            List<WeatherList> result = new List<WeatherList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT TOP 3 * FROM (";
                cmd.CommandText += "    SELECT  TIME, TEMP, RAINFALL, WIND, HUMIDITY, ERNAM ";
                cmd.CommandText += "        FROM PORT_WEATHER "; 
                cmd.CommandText += "    WHERE TIME > '" + strTime + "'";
                cmd.CommandText += " ) t1 ";
                cmd.Parameters.AddWithValue("@strTime", strTime);

                Debug.WriteLine("* SelectWeatherInfo() CommandText : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    WeatherList weatherList = new WeatherList();
                    weatherList.time = rdr["TIME"] as string;
                    weatherList.temp = rdr["TEMP"] as string;
                    weatherList.rainfall = rdr["RAINFALL"] as string;
                    weatherList.wind = rdr["WIND"] as string;
                    weatherList.humidity = rdr["HUMIDITY"] as string;
                    weatherList.ernam = rdr["ERNAM"] as string;
                    
                    result.Add(weatherList);
                }

                return result;
            }

        }

        public List<OrderHistory> SelectOrderHistory(String cust, String fixarrival, String product, String kwmenge, String vadtu, String orderNm)
        {
            SqlDataReader rdr = null;

            string str = "";
            int nTmp;

            DButil.HistoryLog(" SelectOrderHistory1 ");
            if (!string.IsNullOrEmpty(kwmenge)) { 
                string strTarget = kwmenge;
                string strTmp = Regex.Replace(strTarget, @"\D", "");
                str = Regex.Replace(strTarget, @"[\d-]", string.Empty);
                nTmp = int.Parse(strTmp);
            }
            else
            {
                kwmenge = "";
                nTmp = 0;
                str = "";
            }
            DButil.HistoryLog(" SelectOrderHistory2 ");
            List<OrderHistory> orderHistory = new List<OrderHistory>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                if (!string.IsNullOrEmpty(orderNm))
                {
                    cmd.CommandText += " SELECT TOP 1 ";
                    cmd.CommandText += "        KNAME1  AS CUST, ";
                    cmd.CommandText += "        KNAME2   AS FIXARRIVAL, ";
                    cmd.CommandText += "        MAKTX   AS PRODUCT, ";
                    cmd.CommandText += "        CONVERT(VARCHAR(4), KWMENGE)  + VRKME AS KWMENGE, ";
                    cmd.CommandText += "        VDATU, ";
                    cmd.CommandText += "        '인천 1' AS RC, ";
                    cmd.CommandText += "        INFORM ";
                    cmd.CommandText += "    FROM    VOS_ORDER ";
                    cmd.CommandText += "    WHERE   VBELN = @vbeln ";

                    cmd.Parameters.AddWithValue("@vbeln", orderNm);
                } else
                {
                    cmd.CommandText += " SELECT ";
                    cmd.CommandText += " 	( ";
                    if (string.IsNullOrEmpty(cust))
                    {
                        cmd.CommandText += " 		SELECT '' AS KNAME1  ";
                    }
                    else
                    {
                        cmd.CommandText += " 		SELECT TOP 1 KNAME1 FROM BAM_CUST WHERE REPLACE(REPLACE(REPLACE(REPLACE(KNAME1,' ',''),'(주)',''),'식품',''),'주식회사','') LIKE '%" + cust.Replace("(주)", "").Replace("식품", "").Replace("주식회사", "") + "%' OR  KUNNR = '" + cust + "'";
                    }

                    cmd.CommandText += " 	) AS CUST, ";
                    cmd.CommandText += " 	( ";
                    if (string.IsNullOrEmpty(fixarrival))
                    {
                        cmd.CommandText += " 		SELECT '' AS KNAME1  ";
                    }
                    else
                    {
                        cmd.CommandText += " 		SELECT TOP 1 KNAME1  FROM BAM_FIXARRIVAL WHERE REPLACE(REPLACE(REPLACE(REPLACE(KNAME1,' ',''),'(주)',''),'식품',''),'주식회사','') LIKE '%" + fixarrival.Replace("(주)", "").Replace("식품", "").Replace("주식회사", "") + "%' OR KUNN2 = '" + fixarrival + "'";
                    }

                    cmd.CommandText += " 	) AS FIXARRIVAL, ";
                    cmd.CommandText += " 	( ";
                    if (string.IsNullOrEmpty(product))
                    {
                        cmd.CommandText += " 		SELECT '' AS MAKTX ";
                    }
                    else
                    {
                        cmd.CommandText += " 		SELECT MAKTX   FROM BAM_PRODUCT WHERE REPLACE(MAKTXC,' ','') LIKE '%" + product.Replace(" ", "").Replace("/상온", "") + "%' ";
                        cmd.CommandText += " 		                                    OR REPLACE(MAKTXC,' ','') LIKE '%" + product.Replace(" ", "")+ "%' OR MATNR = '" + product + "' ";
                        cmd.CommandText += " 		                                    OR REPLACE(MAKTX,' ','') = '" + product.Replace(" ", "") + "' ";
                    }
                    cmd.CommandText += " 	) AS PRODUCT, ";
                    if (string.IsNullOrEmpty(kwmenge))
                    {
                        cmd.CommandText += " 	'' AS KWMENGE, ";
                    }
                    else
                    {
                        cmd.CommandText += " 	CONVERT(VARCHAR(4),@kwmenge)+(SELECT ENTITY FROM BAM_MEASURE WHERE ENTITY_VALUE = @uint) AS KWMENGE, ";
                    }
                    
                    cmd.CommandText += " 	( ";
                    if (string.IsNullOrEmpty(vadtu))
                    {
                        cmd.CommandText += " 		SELECT  '' ";
                    }
                    else
                    {
                        cmd.CommandText += " 		SELECT  '2018.'+STUFF( ";
                        cmd.CommandText += "(SELECT '.', RIGHT('0' + REPLACE(REPLACE(VAL1, '월', ''), '일', ''), 2) FROM SPLIT2(REPLACE(@vadtu,'월',' '), ' ') FOR XML PATH('')) ";
                        cmd.CommandText += "		,1,1,'') ";
                    }

                    cmd.CommandText += " 	) AS VDATU ";

                    cmd.Parameters.AddWithValue("@cust", cust);
                    //cmd.Parameters.AddWithValue("@fixarrival", fixarrival);
                    //cmd.Parameters.AddWithValue("@product", product);

                    cmd.Parameters.AddWithValue("@kwmenge", nTmp);
                    cmd.Parameters.AddWithValue("@uint", str);
                    cmd.Parameters.AddWithValue("@vadtu", vadtu);
                }
                

                Debug.WriteLine("query : " + cmd.CommandText);
                DButil.HistoryLog("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string custValue = rdr["CUST"] as string;
                    string fixarrivalValue = rdr["FIXARRIVAL"] as string;
                    string productValue = rdr["PRODUCT"] as string;
                    string kwmengeValue = rdr["KWMENGE"] as string;
                    string vadtuValue = rdr["VDATU"] as string;

                    OrderHistory dlg = new OrderHistory();
                    dlg.cust = custValue;
                    dlg.fixarrival = fixarrivalValue;
                    dlg.product = productValue;
                    dlg.kwmenge = kwmengeValue;
                    dlg.vdatu = vadtuValue;

                    orderHistory.Add(dlg);
                }
            }
            return orderHistory;
        }

        public int insertOrder(String cust, String fixarrival, String product, String kwmenge, String vadtu, String inform, String emp_no) 
        {
            int result;
            string strTarget = kwmenge;
            string strTmp = Regex.Replace(strTarget, @"\D", "");
            string str = Regex.Replace(strTarget, @"[\d-]", string.Empty);
            int nTmp = int.Parse(strTmp);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;



                //cmd.CommandText += " INSERT INTO VOS_ORDER ";
                //cmd.CommandText += " (VBELN, VBELN_SEQ, KUNNR, KNAME1, KUNN2, KNAME2, VDATU, INFORM, MATNR, MAKTX, KWMENGE, VRKME, EMP_NO) ";
                //cmd.CommandText += " VALUES ";
                //cmd.CommandText += " (  (SELECT MAX(VBELN)+1 FROM VOS_ORDER) ";
                //cmd.CommandText += "    , '1' ";
                //cmd.CommandText += "    , REPLACE(RIGHT(@cust, CHARINDEX('(', REVERSE(@cust))-1),')','') ";
                //cmd.CommandText += "    , REPLACE(@cust,'('+RIGHT(@cust, CHARINDEX('(', REVERSE(@cust))-1),'') ";
                //cmd.CommandText += "    , REPLACE(RIGHT(@fixarrival, CHARINDEX('(', REVERSE(@fixarrival))-1),')','') ";
                //cmd.CommandText += "    , REPLACE(@fixarrival,'('+RIGHT(@fixarrival, CHARINDEX('(', REVERSE(@fixarrival))-1),'') ";
                //cmd.CommandText += "    , REPLACE(@vadtu,'.','') ";
                //cmd.CommandText += "    , @inform ";
                //cmd.CommandText += "    , REPLACE(RIGHT(@product, CHARINDEX('(', REVERSE(@product))-1),')','') ";
                //cmd.CommandText += "    , REPLACE(@product,'('+RIGHT(@product, CHARINDEX('(', REVERSE(@product))-1),'') ";
                //cmd.CommandText += "    , @kwmenge ";
                //cmd.CommandText += "    , (SELECT ENTITY FROM BAM_MEASURE WHERE ENTITY_VALUE = @uint) ";
                //cmd.CommandText += "    , @emp_no) ";


                cmd.CommandText += " INSERT INTO VOS_ORDER ";
                cmd.CommandText += " (VBELN, VBELN_SEQ, KUNNR, KNAME1, KUNN2, KNAME2, VDATU, INFORM, MATNR, MAKTX, KWMENGE, VRKME, EMP_NO, REGDATE, API_FLAG) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (  (SELECT MAX(VBELN)+1 FROM VOS_ORDER) ";
                cmd.CommandText += "    , '1' ";
                cmd.CommandText += "    , (SELECT KUNNR FROM BAM_CUST WHERE KNAME1 = @cust) ";
                cmd.CommandText += "    , @cust ";
                cmd.CommandText += "    , (SELECT KUNNR FROM BAM_FIXARRIVAL WHERE KNAME1 = @fixarrival) ";
                cmd.CommandText += "    , @fixarrival ";
                cmd.CommandText += "    , REPLACE(@vadtu,'.','') ";
                cmd.CommandText += "    , @inform ";
                cmd.CommandText += "    , (SELECT MATNR FROM BAM_PRODUCT WHERE MAKTX = @product) ";
                cmd.CommandText += "    , @product ";
                cmd.CommandText += "    , @kwmenge ";
                cmd.CommandText += "    , (SELECT ENTITY FROM BAM_MEASURE WHERE ENTITY_VALUE = @uint) ";
                cmd.CommandText += "    , @emp_no, CONVERT(VARCHAR(8),GETDATE(),112), '0') ";

                cmd.Parameters.AddWithValue("@cust", cust);
                cmd.Parameters.AddWithValue("@fixarrival", fixarrival);
                cmd.Parameters.AddWithValue("@product", product);
                cmd.Parameters.AddWithValue("@kwmenge", nTmp);
                cmd.Parameters.AddWithValue("@uint", str);
                cmd.Parameters.AddWithValue("@vadtu", vadtu);
                cmd.Parameters.AddWithValue("@inform", inform);
                cmd.Parameters.AddWithValue("@emp_no", emp_no);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        public int updateOrder(String vbeln, String cust, String fixarrival, String product, String kwmenge, String vadtu, String inform)
        {
            int result;
            string strTarget = kwmenge;
            string strTmp = Regex.Replace(strTarget, @"\D", "");
            string str = Regex.Replace(strTarget, @"[\d-]", string.Empty);
            int nTmp = int.Parse(strTmp);


            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " UPDATE	VOS_ORDER ";
                cmd.CommandText += " SET ";
                cmd.CommandText += "        KUNNR = (SELECT KUNNR FROM BAM_CUST WHERE KNAME1 = @cust), ";
                cmd.CommandText += "        KNAME1 = @cust,  ";
                cmd.CommandText += "        KUNN2 = (SELECT KUNNR FROM BAM_FIXARRIVAL WHERE KNAME1 = @fixarrival), ";
                cmd.CommandText += "        KNAME2 = @fixarrival,  ";
                cmd.CommandText += "        VDATU = REPLACE(@vadtu, '.', ''), ";
                cmd.CommandText += "        INFORM = @inform, ";
                cmd.CommandText += "        MATNR = (SELECT MATNR FROM BAM_PRODUCT WHERE MAKTX = @product), ";
                cmd.CommandText += "        MAKTX = @product, ";
                cmd.CommandText += "        KWMENGE = @kwmenge, ";
                cmd.CommandText += "        VRKME = (SELECT ENTITY FROM BAM_MEASURE WHERE ENTITY_VALUE = @uint) ";
                
                cmd.CommandText += " WHERE  VBELN = @vbeln ";

                cmd.Parameters.AddWithValue("@cust", cust);
                cmd.Parameters.AddWithValue("@fixarrival", fixarrival);
                cmd.Parameters.AddWithValue("@product", product);
                cmd.Parameters.AddWithValue("@kwmenge", nTmp);
                cmd.Parameters.AddWithValue("@uint", str);
                cmd.Parameters.AddWithValue("@vadtu", vadtu);
                cmd.Parameters.AddWithValue("@inform", inform);
                cmd.Parameters.AddWithValue("@vbeln", vbeln);

                Debug.WriteLine("query : " + cmd.CommandText);
                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        public List<OrderList> SelectOrderList(string cust, String vadtu, String emp_no)
        {
            SqlDataReader rdr = null;
            List<OrderList> orderList = new List<OrderList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //cmd.CommandText = "     SELECT	 ";
                //cmd.CommandText += "         TOP 1   ";
                //cmd.CommandText += "         (SELECT KNAME1 FROM BAM_CUST WHERE KUNNR = A.KUNNR) + '(' + A.KUNNR + ')' AS CUST, ";
                //cmd.CommandText += "         (SELECT TOP 1 KNAME1 FROM BAM_FIXARRIVAL WHERE KUNNR = A.KUNEWE) +'(' + A.KUNEWE + ')' AS FIXARRIVAL, ";
                //cmd.CommandText += "         (SELECT MAKTX + '(' + MATNR + ')'   FROM BAM_PRODUCT WHERE REPLACE(MAKTX, ' ', '') LIKE '%' + C.MAKTX + '%' ) AS PRODUCT, ";
                //cmd.CommandText += "         CONVERT(VARCHAR(1), B.KWMENGE) +' ' + B.VRKME AS KWMENGE, ";
                //cmd.CommandText += "         CONVERT(CHAR(10), CONVERT(DATE, A.VDATU), 102) AS VDATU, ";                
                //cmd.CommandText += "         '인천 1' AS RC, ";
                //cmd.CommandText += "         A.INFORM ";
                //cmd.CommandText += "     FROM    ORT_ORDER A, ORT_ORDERDETAIL B, VOS_ORDER C ";
                //cmd.CommandText += "     WHERE   A.VBELN = B.VBELN ";
                //cmd.CommandText += "     AND     B.VBELN = C.VBELN ";
                //cmd.CommandText += "     AND     A.KNAME1 LIKE '%"+cust+"%' ";
                //cmd.CommandText += "     AND     A.VDATU = ( ";
                //cmd.CommandText += "                        SELECT REPLACE('2018' + STUFF( ";
                //cmd.CommandText += "                            (SELECT '.', RIGHT('0' + REPLACE(REPLACE(VAL1, '월', ''), '일', ''), 2) FROM SPLIT2(REPLACE('"+ vadtu + "', '월', ' '), ' ') FOR XML PATH('')) ";
                //cmd.CommandText += "                            ,1,1,''),'.','') ) ";

                cmd.CommandText = "     SELECT CONVERT(VARCHAR(100),VBELN) AS VBELN, CONVERT(VARCHAR(100),VBELN_SEQ) AS VBELN_SEQ, ";
                cmd.CommandText += "        KNAME1 AS CUST, KUNNR AS CUST_NR, ";
                cmd.CommandText += "        KNAME2 AS FIXARRIVAL, KUNN2 AS FIXARRIVAL_NR, ";
                cmd.CommandText += "        MAKTX AS PRODUCT, MATNR AS PRODUCT_NR, ";
                cmd.CommandText += "        CONVERT(VARCHAR(4), KWMENGE)  +VRKME AS KWMENGE, ";
                cmd.CommandText += "        CONVERT(CHAR(10), CONVERT(DATE, VDATU), 102) AS VDATU, ";
                cmd.CommandText += "        '인천 1' AS RC, ";
                cmd.CommandText += "        INFORM ";
                cmd.CommandText += "    FROM    VOS_ORDER ";
                cmd.CommandText += "    WHERE   (REPLACE(REPLACE(REPLACE(REPLACE(KNAME1,' ',''),'(주)',''),'식품',''),'주식회사','') LIKE '%" + cust.Replace("(주)", "").Replace("식품", "").Replace("주식회사", "") + "%' ";
                cmd.CommandText += "    OR      KUNNR LIKE '%" + cust + "%') ";
                cmd.CommandText += "    AND     EMP_NO = @emp_no ";
                cmd.CommandText += "    AND     VDATU = ( ";
                cmd.CommandText += "                    SELECT REPLACE('2018' + STUFF( ";
                cmd.CommandText += "                        (SELECT '.', RIGHT('0' + REPLACE(REPLACE(VAL1, '월', ''), '일', ''), 2) FROM SPLIT2(REPLACE(@vadtu, '월', ' '), ' ') FOR XML PATH('')) ";
                cmd.CommandText += "                        , 1, 1, ''),'.','') ) ";
                cmd.CommandText += "    ORDER BY VBELN_SEQ DESC ";

                //cmd.Parameters.AddWithValue("@cust", cust);
                cmd.Parameters.AddWithValue("@vadtu", vadtu);
                cmd.Parameters.AddWithValue("@emp_no", emp_no);

                Debug.WriteLine("query : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string vbelnValue = rdr["VBELN"] as string;
                    string vbelnSeqValue = rdr["VBELN_SEQ"] as string;
                    string custValue = rdr["CUST"] as string;
                    string custNrValue = rdr["CUST_NR"] as string;
                    string fixarrivalValue = rdr["FIXARRIVAL"] as string;
                    string fixarrivalNrValue = rdr["FIXARRIVAL_NR"] as string;
                    string productValue = rdr["PRODUCT"] as string;
                    string productNrValue = rdr["PRODUCT_NR"] as string;
                    string kwmengeValue = rdr["KWMENGE"] as string;
                    string vadtuValue = rdr["VDATU"] as string;
                    string rcValue = rdr["RC"] as string;
                    string informValue = rdr["INFORM"] as string;

                    OrderList order = new OrderList();

                    order.vbeln = vbelnValue;
                    order.vbeln_seq = vbelnSeqValue;
                    order.cust = custValue;
                    order.cust_nr = custNrValue;
                    order.fixarrival = fixarrivalValue;
                    order.fixarrival_nr = fixarrivalNrValue;
                    order.product = productValue;
                    order.product_nr = productNrValue;
                    order.kwmenge = kwmengeValue;
                    order.rc = rcValue;
                    order.vdatu = vadtuValue;
                    order.inform = informValue;

                    orderList.Add(order);
                }
            }
            return orderList;
        }

        public int deleteOrder(String vbeln)
        {
            int result;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " DELETE FROM VOS_ORDER ";
                cmd.CommandText += "    WHERE VBELN = @vbeln   ";

                cmd.Parameters.AddWithValue("@vbeln", vbeln);

                Debug.WriteLine("query : " + cmd.CommandText);
                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }
        
        public List<ClientList> SelectClientList(String emp_no)
        {
            SqlDataReader rdr = null;
            List<ClientList> clientList = new List<ClientList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "     SELECT TOP 5 ";
                cmd.CommandText += "        KUNNR, KNAME1 ";
                cmd.CommandText += "    FROM    BAM_CUST ";
                cmd.CommandText += "    WHERE	PERNR = @emp_no ";
                cmd.CommandText += "    GROUP	BY KUNNR, KNAME1 ";

                cmd.Parameters.AddWithValue("@emp_no", emp_no);

                Debug.WriteLine("query : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {

                    //string vbelnSeqValue = rdr["VBELN_SEQ"] as string;
                    string kunnrValue = rdr["KUNNR"] as string;
                    string kname1Value = rdr["KNAME1"] as string;

                    ClientList clientlist = new ClientList();

                    //clientlist.vbeln_seq = vbelnSeqValue;
                    clientlist.kunnr = kunnrValue;
                    clientlist.kname1 = kname1Value;

                    clientList.Add(clientlist);
                }
            }
            return clientList;
        }
        
        public List<ClientList> SelectFixarrivalList()
        {
            SqlDataReader rdr = null;
            List<ClientList> clientList = new List<ClientList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "     SELECT TOP 5 ";
                cmd.CommandText += "        KUNNR, KNAME1 ";
                cmd.CommandText += "    FROM    BAM_FIXARRIVAL ";
                //cmd.CommandText += "    WHERE	PERNR = @emp_no ";
                cmd.CommandText += "    GROUP	BY KUNNR, KNAME1 ";

                //cmd.Parameters.AddWithValue("@emp_no", emp_no);

                Debug.WriteLine("query : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {

                    //string vbelnSeqValue = rdr["VBELN_SEQ"] as string;
                    string kunnrValue = rdr["KUNNR"] as string;
                    string kname1Value = rdr["KNAME1"] as string;

                    ClientList clientlist = new ClientList();

                    //clientlist.vbeln_seq = vbelnSeqValue;
                    clientlist.kunnr = kunnrValue;
                    clientlist.kname1 = kname1Value;

                    clientList.Add(clientlist);
                }
            }
            return clientList;
        }

        public List<PastOrderList> SelectPastList(String cust, String fixarrival, String product, String emp_no, String vdatu)
        {
            SqlDataReader rdr = null;
            List<PastOrderList> pastOrderList = new List<PastOrderList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "     SELECT CONVERT(VARCHAR(100),VBELN_SEQ) AS VBELN_SEQ, ";
                cmd.CommandText += "        A.KNAME1 AS CUST, ";
                cmd.CommandText += "        A.KNAME2   AS FIXARRIVAL, ";
                cmd.CommandText += "        A.MAKTX   AS PRODUCT, ";
                cmd.CommandText += "        CONVERT(VARCHAR(4), A.KWMENGE)  + A.VRKME AS KWMENGE, ";
                cmd.CommandText += "        (SELECT  '2018.'+STUFF( ";
                cmd.CommandText += "            (SELECT '.', RIGHT('0' + REPLACE(REPLACE(VAL1, '월', ''), '일', ''), 2) FROM SPLIT2(REPLACE(@vdatu, '월', ' '), ' ') FOR XML PATH('')) ";
                cmd.CommandText += "            ,1,1,'')) AS VDATU, ";
                cmd.CommandText += "        '인천 1' AS RC, ";
                cmd.CommandText += "        A.INFORM ";
                cmd.CommandText += "    FROM    VOS_ORDER A, BAM_PRODUCT B ";
                cmd.CommandText += "    WHERE   A.MATNR = B.MATNR";
                cmd.CommandText += "    AND     REPLACE(REPLACE(REPLACE(REPLACE(A.KNAME1,' ',''),'(주)',''),'식품',''),'주식회사','') LIKE '%" + cust.Replace("(주)", "").Replace("식품", "").Replace("주식회사", "") + "%' ";
                cmd.CommandText += "    AND     A.EMP_NO = @emp_no ";
                cmd.CommandText += "    AND     B.MAKTXC LIKE '%" + product.Replace(" ", "") + "%' ";

                cmd.Parameters.AddWithValue("@emp_no", emp_no);
                cmd.Parameters.AddWithValue("@vdatu", vdatu);
                
                Debug.WriteLine("query : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {

                    string custValue = rdr["CUST"] as string;
                    string fixarrivalValue = rdr["FIXARRIVAL"] as string;
                    string productValue = rdr["PRODUCT"] as string;
                    string kwmengeValue = rdr["KWMENGE"] as string;
                    string vadtuValue = rdr["VDATU"] as string;

                    PastOrderList pastorderlist = new PastOrderList();

                    pastorderlist.cust = custValue;
                    pastorderlist.fixarrival = fixarrivalValue;
                    pastorderlist.product = productValue;
                    pastorderlist.kwmenge = kwmengeValue;
                    pastorderlist.vdatu = vadtuValue;

                    pastOrderList.Add(pastorderlist);
                }
            }
            return pastOrderList;
        }

        public List<OrderList> SearchOrderList(string vbeln)
        {
            SqlDataReader rdr = null;
            List<OrderList> orderList = new List<OrderList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "     SELECT CONVERT(VARCHAR(100),VBELN) AS VBELN, CONVERT(VARCHAR(100),VBELN_SEQ) AS VBELN_SEQ, ";
                cmd.CommandText += "        KNAME1 AS CUST, KUNNR AS CUST_NR, ";
                cmd.CommandText += "        KNAME2 AS FIXARRIVAL, KUNN2 AS FIXARRIVAL_NR, ";
                cmd.CommandText += "        MAKTX AS PRODUCT, MATNR AS PRODUCT_NR, ";
                cmd.CommandText += "        CONVERT(VARCHAR(4), KWMENGE)  +VRKME AS KWMENGE, ";
                cmd.CommandText += "        CONVERT(CHAR(10), CONVERT(DATE, VDATU), 102) AS VDATU, ";
                cmd.CommandText += "        '인천 1' AS RC, ";
                cmd.CommandText += "        INFORM ";
                cmd.CommandText += "    FROM    VOS_ORDER ";
                cmd.CommandText += "    WHERE   VBELN = @vbeln ";
                cmd.CommandText += "    AND     EMP_NO = @emp_no ";

                cmd.Parameters.AddWithValue("@vbeln", vbeln);

                Debug.WriteLine("query : " + cmd.CommandText);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string vbelnValue = rdr["VBELN"] as string;
                    string vbelnSeqValue = rdr["VBELN_SEQ"] as string;
                    string custValue = rdr["CUST"] as string;
                    string custNrValue = rdr["CUST_NR"] as string;
                    string fixarrivalValue = rdr["FIXARRIVAL"] as string;
                    string fixarrivalNrValue = rdr["FIXARRIVAL_NR"] as string;
                    string productValue = rdr["PRODUCT"] as string;
                    string productNrValue = rdr["PRODUCT_NR"] as string;
                    string kwmengeValue = rdr["KWMENGE"] as string;
                    string vadtuValue = rdr["VDATU"] as string;
                    string rcValue = rdr["RC"] as string;
                    string informValue = rdr["INFORM"] as string;

                    OrderList order = new OrderList();

                    order.vbeln = vbelnValue;
                    order.vbeln_seq = vbelnSeqValue;
                    order.cust = custValue;
                    order.cust_nr = custNrValue;
                    order.fixarrival = fixarrivalValue;
                    order.fixarrival_nr = fixarrivalNrValue;
                    order.product = productValue;
                    order.product_nr = productNrValue;
                    order.kwmenge = kwmengeValue;
                    order.rc = rcValue;
                    order.vdatu = vadtuValue;
                    order.inform = informValue;

                    orderList.Add(order);
                }
            }
            return orderList;
        }
    }
}