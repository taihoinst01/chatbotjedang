using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using PortChatBot.DB;
using PortChatBot.Models;
using Newtonsoft.Json.Linq;

using System.Configuration;
using System.Web.Configuration;
using PortChatBot.Dialogs;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.ConnectorEx;

namespace PortChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public static readonly string TEXTDLG = "2";
        public static readonly string CARDDLG = "3";
        public static readonly string MEDIADLG = "4";
        public static readonly int MAXFACEBOOKCARDS = 10;

        public static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/");
        const string chatBotAppID = "appID";
        public static int appID = Convert.ToInt32(rootWebConfig.ConnectionStrings.ConnectionStrings[chatBotAppID].ToString());

        public static string subscriptionKey = "353c57887e05492485845aa47759c25b";

        //config 변수 선언
        static public string[] LUIS_NM = new string[10];        //루이스 이름
        static public string[] LUIS_APP_ID = new string[10];    //루이스 app_id
        static public string LUIS_SUBSCRIPTION = "";            //루이스 구독키
        static public int LUIS_TIME_LIMIT;                      //루이스 타임 체크
        static public string QUOTE = "";                        //견적 url
        static public string TESTDRIVE = "";                    //시승 url
        static public string BOT_ID = "";                       //bot id
        static public string MicrosoftAppId = "";               //app id
        static public string MicrosoftAppPassword = "";         //app password
        static public string LUIS_SCORE_LIMIT = "";             //루이스 점수 체크

        public static int sorryMessageCnt = 0;
        public static int chatBotID = 0;

        public static int pagePerCardCnt = 10;
        public static int pageRotationCnt = 0;
        public static int fbLeftCardCnt = 0;
        public static int facebookpagecount = 0;
        public static string FB_BEFORE_MENT = "";

        public static List<AnalysisList> analysisList = new List<AnalysisList>();
        public static List<TrendList> trendList = new List<TrendList>();
        public static List<HrList> hrList = new List<HrList>();
        public static List<WeatherList> weatherList = new List<WeatherList>();
        public static List<RelationList> relationList = new List<RelationList>();
        public static string luisId = "";
        public static string luisIntent = "";
        public static string luisEntities = "";
        public static string luisEntitiesValue = ""; 
        public static string queryStr = "";
        public static DateTime startTime;

        //주문내역
        public static string cust = "";
        public static string kunnr = "";
        public static string matnr = "";
        public static string kwmenge = "";
        public static string vdatu = "";
        public static string inform = "";
        public static string selectYn = "";
        public static string vbeln = "";
        public static string vbeln_seq = "";
        public static string rc = "";
        public static string orderNm = "";
        public static string pastList = "";

        //부분TTS
        public static int ttsCnt = 0;
        
        public static CacheList cacheList = new CacheList();
        public static QueryIntentList queryIntentList = new QueryIntentList();
        //페이스북 페이지용
        public static ConversationHistory conversationhistory = new ConversationHistory();
        //추천 컨텍스트 분석용
        public static Dictionary<String, String> recommenddic = new Dictionary<string, String>();
        //결과 플레그 H : 정상 답변, S : 기사검색 답변, D : 답변 실패
        public static String replyresult = "";
        //API 플레그 QUOT : 견적, TESTDRIVE : 시승 RECOMMEND : 추천 COMMON : 일반 SEARCH : 검색
        public static String apiFlag = "";
        public static String recommendResult = "";

        public static string channelID = "";

        public static DbConnect db = new DbConnect();
        public static DButil dbutil = new DButil();

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            string cashOrgMent = "";

            //DbConnect db = new DbConnect();
            //DButil dbutil = new DButil();
            DButil.HistoryLog("db connect !! " );
            //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            HttpResponseMessage response ;

            Activity reply1 = activity.CreateReply();
            Activity reply2 = activity.CreateReply();
            Activity reply3 = activity.CreateReply();
            Activity reply4 = activity.CreateReply();

            // Activity 값 유무 확인하는 익명 메소드
            Action<Activity> SetActivity = (act) =>
            {
                if (!(reply1.Attachments.Count != 0 || reply1.Text != ""))
                {
                    reply1 = act;
                }
                else if (!(reply2.Attachments.Count != 0 || reply2.Text != ""))
                {
                    reply2 = act;
                }
                else if (!(reply3.Attachments.Count != 0 || reply3.Text != ""))
                {
                    reply3 = act;
                }
                else if (!(reply4.Attachments.Count != 0 || reply4.Text != ""))
                {
                    reply4 = act;
                }
                else
                {

                }
            };

            //DButil.HistoryLog("activity.Recipient.Name : " + activity.Recipient.Name);
            //DButil.HistoryLog("activity.Name : " + activity.Name);

            // userData (epkim)
            StateClient stateClient = activity.GetStateClient();
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            DButil.HistoryLog("db connect !!1 ");
            if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                startTime = DateTime.Now;
                //activity.ChannelId = "facebook";
                //파라메터 호출
                if (LUIS_NM.Count(s => s != null) > 0)
                {
                    //string[] LUIS_NM = new string[10];
                    Array.Clear(LUIS_NM, 0, LUIS_NM.Length);
                }

                if (LUIS_APP_ID.Count(s => s != null) > 0)
                {
                    //string[] LUIS_APP_ID = new string[10];
                    Array.Clear(LUIS_APP_ID, 0, LUIS_APP_ID.Length);
                }
                //Array.Clear(LUIS_APP_ID, 0, 10);
                DButil.HistoryLog("db SelectConfig start !! ");
                List<ConfList> confList = db.SelectConfig();
                DButil.HistoryLog("db SelectConfig end!! ");

                //
                userData.SetProperty<string>("loginStatus", "");
                userData.SetProperty<string>("emp_no", "");
                userData.SetProperty<string>("user_nm", "");
                userData.SetProperty<string>("dept_cd", "");
                userData.SetProperty<string>("dept_nm", "");
                userData.SetProperty<string>("user_id", "");

                userData.SetProperty<string>("cust", "");
                userData.SetProperty<string>("kunnr", "");
                userData.SetProperty<string>("matnr", "");
                userData.SetProperty<string>("kwmenge", "");
                userData.SetProperty<string>("vdatu", "");
                userData.SetProperty<string>("inform", "");
                userData.SetProperty<string>("rc", "");
                userData.SetProperty<string>("vblen_seq", "");
                userData.SetProperty<string>("selectYn", "");


                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                //

                for (int i = 0; i < confList.Count; i++)
                {
                    switch (confList[i].cnfType)
                    {
                        case "LUIS_APP_ID":
                            LUIS_APP_ID[LUIS_APP_ID.Count(s => s != null)] = confList[i].cnfValue;
                            LUIS_NM[LUIS_NM.Count(s => s != null)] = confList[i].cnfNm;
                            break;
                        case "LUIS_SUBSCRIPTION":
                            LUIS_SUBSCRIPTION = confList[i].cnfValue;
                            break;
                        case "BOT_ID":
                            BOT_ID = confList[i].cnfValue;
                            break;
                        case "MicrosoftAppId":
                            MicrosoftAppId = confList[i].cnfValue;
                            break;
                        case "MicrosoftAppPassword":
                            MicrosoftAppPassword = confList[i].cnfValue;
                            break;
                        case "QUOTE":
                            QUOTE = confList[i].cnfValue;
                            break;
                        case "TESTDRIVE":
                            TESTDRIVE = confList[i].cnfValue;
                            break;
                        case "LUIS_SCORE_LIMIT":
                            LUIS_SCORE_LIMIT = confList[i].cnfValue;
                            break;
                        case "LUIS_TIME_LIMIT":
                            LUIS_TIME_LIMIT = Convert.ToInt32(confList[i].cnfValue);
                            break;
                        default: //미 정의 레코드
                            Debug.WriteLine("* conf type : " + confList[i].cnfType + "* conf value : " + confList[i].cnfValue);
                            DButil.HistoryLog("* conf type : " + confList[i].cnfType + "* conf value : " + confList[i].cnfValue);
                            break;
                    }
                }

                Debug.WriteLine("* DB conn : " + activity.Type);
                DButil.HistoryLog("* DB conn : " + activity.Type);

                //초기 다이얼로그 호출
                List<DialogList> dlg = db.SelectInitDialog(activity.ChannelId);

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                foreach (DialogList dialogs in dlg)
                {
                    Activity initReply = activity.CreateReply();
                    initReply.Recipient = activity.From;
                    initReply.Type = "message";
                    initReply.Attachments = new List<Attachment>();
                    //initReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    Attachment tempAttachment;

                    if (dialogs.dlgType.Equals(CARDDLG))
                    {
                        foreach (CardList tempcard in dialogs.dialogCard)
                        {
                            tempAttachment = dbutil.getAttachmentFromDialog(tempcard, activity);
                            initReply.Attachments.Add(tempAttachment);
                        }
                    }
                    else
                    {
                        DButil.HistoryLog("* ConversationUpdate : dlgType is not CARDDLG ");
                        if (activity.ChannelId.Equals("facebook") && string.IsNullOrEmpty(dialogs.cardTitle) && dialogs.dlgType.Equals(TEXTDLG))
                        {
                            DButil.HistoryLog("* ConversationUpdate : dlgType is not CARDDLG | facebook");
                            Activity reply_facebook = activity.CreateReply();
                            reply_facebook.Recipient = activity.From;
                            reply_facebook.Type = "message";
                            DButil.HistoryLog("facebook card Text : " + dialogs.cardText);
                            reply_facebook.Text = dialogs.cardText;
                            var reply_ment_facebook = connector.Conversations.SendToConversationAsync(reply_facebook);
                            //SetActivity(reply_facebook);

                        }
                        else
                        {
                            DButil.HistoryLog("* ConversationUpdate : dlgType is not CARDDLG | NOT facebook");
                            tempAttachment = dbutil.getAttachmentFromDialog(dialogs, activity);
                            initReply.Attachments.Add(tempAttachment);
                        }
                    }
                    await connector.Conversations.SendToConversationAsync(initReply);
                }

                DateTime endTime = DateTime.Now;
                Debug.WriteLine("프로그램 수행시간 : {0}/ms", ((endTime - startTime).Milliseconds));
                Debug.WriteLine("* activity.Type : " + activity.Type);
                Debug.WriteLine("* activity.Recipient.Id : " + activity.Recipient.Id);
                Debug.WriteLine("* activity.ServiceUrl : " + activity.ServiceUrl);

                DButil.HistoryLog("* activity.Type : " + activity.ChannelData);
                DButil.HistoryLog("* activity.Recipient.Id : " + activity.Recipient.Id);
                DButil.HistoryLog("* activity.ServiceUrl : " + activity.ServiceUrl);
                DButil.HistoryLog("* activity.attachments.content.title : " + activity.Attachments);
            }
            else if (activity.Type == ActivityTypes.Message)
            {

                DButil.HistoryLog("* activity.Type == ActivityTypes.Message ");
                // userData (epkim)
                //StateClient stateClient = activity.GetStateClient();
                //BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                //activity.ChannelId = "facebook";
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                try
                {
                    Debug.WriteLine("* activity.Type == ActivityTypes.Message ");
                    channelID = activity.ChannelId;
                    string orgMent = activity.Text.TrimStart(' ');
                    apiFlag = "COMMON";

                    //대화 시작 시간
                    startTime = DateTime.Now;
                    long unixTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds();                    

                    DButil.HistoryLog("*** orgMent : " + orgMent);
                    //금칙어 체크
                    CardList bannedMsg = db.BannedChk(orgMent);
                    Debug.WriteLine("* bannedMsg : " + bannedMsg.cardText);//해당금칙어에 대한 답변

                    if (bannedMsg.cardText != null)
                    {
                        Activity reply_ment = activity.CreateReply();
                        reply_ment.Recipient = activity.From;
                        reply_ment.Type = "message";
                        reply_ment.Text = bannedMsg.cardText;

                        var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                        response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        Debug.WriteLine("* NO bannedMsg !");
                        /*
                        string strDetectedLang = "";
                        strDetectedLang = await DetectLang(orgMent);
                        Debug.WriteLine("* DetectLang : "+strDetectedLang);
                        */
                        queryStr = orgMent;
                        //인텐트 엔티티 검출
                        //캐시 체크
                        cashOrgMent = Regex.Replace(orgMent, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline);
                        cacheList = db.CacheChk(cashOrgMent.Replace(" ", ""));                     // 캐시 체크 (TBL_QUERY_ANALYSIS_RESULT 조회..)
                        
                        //캐시에 없을 경우
                        if (cacheList.luisIntent == null || cacheList.luisEntities == null)
                        {
                            DButil.HistoryLog("cache none : " + orgMent);
                            Debug.WriteLine("cache none : " + orgMent);
                            //루이스 체크(intent를 루이스를 통해서 가져옴)
                            //cacheList.luisId = dbutil.GetMultiLUIS(orgMent);
                            //Debug.WriteLine("cacheList.luisId : " + cacheList.luisId);

                            cacheList.luisIntent = dbutil.GetMultiLUIS(orgMent);
                            Debug.WriteLine("cacheList.luisIntent : " + cacheList.luisIntent);
                            Debug.WriteLine("cacheList.luisEntitiesValue : " + cacheList.luisEntitiesValue);
                            cacheList = db.CacheDataFromIntent(cacheList.luisIntent, cacheList.luisEntitiesValue);


                        }
                        
                        if (cacheList != null && cacheList.luisIntent != null)
                        {
                            if (cacheList.luisIntent.Contains("testdrive") || cacheList.luisIntent.Contains("branch"))
                            {
                                apiFlag = "TESTDRIVE";
                            }
                            else if (cacheList.luisIntent.Contains("quot"))
                            {
                                apiFlag = "QUOT"; 
                            }
                            else if (cacheList.luisIntent.Contains("recommend "))
                            {
                                apiFlag = "RECOMMEND";
                            }
                            else 
                            {
                                apiFlag = "COMMON";
                            }
                            DButil.HistoryLog("cacheList.luisIntent : " + cacheList.luisIntent);
                            Debug.WriteLine("cacheList.luisIntent : " + cacheList.luisIntent);
                        }

                        luisId = cacheList.luisId;
                        luisIntent = cacheList.luisIntent;
                        luisEntities = cacheList.luisEntities;
                        luisEntitiesValue = cacheList.luisEntitiesValue;

                        if(luisIntent=="주문완료" && luisEntities == "주문완료")
                        {
                            ttsCnt = 0;
                        }


                        Debug.WriteLine("* cacheList luisId: " + luisId + " | luisIntent : "+ luisIntent+ " | luisEntities : "+ luisEntities);

                        if(luisId == null || luisIntent == null)
                        {
                            Debug.WriteLine("* cacheList is NULL | cashOrgMent : " + cashOrgMent);
                            
                            //cacheList = db.SelectQueryIntent(cashOrgMent.Replace(" ", ""));                     // 캐시 체크 (TBL_QUERY_ANALYSIS_RESULT 조회..)
                            //Debug.WriteLine("* cacheList luisId: " + cacheList.luisId + " | luisIntent : " + cacheList.luisIntent + " | luisEntities : " + cacheList.luisEntities);
                        }


                        String fullentity = db.SearchCommonEntities;    //  FN_ENTITY_ORDERBY_ADD
                        DButil.HistoryLog("***** fullentity first : " + fullentity);
                        Debug.WriteLine("***** fullentity first : " + fullentity);
                        if (fullentity.Equals(""))
                        {
                            var loginStatus = userData.GetProperty<string>("loginStatus");
                            int n = 0;
                            var isNumeric = int.TryParse(orgMent, out n);
                            if (orgMent.Contains("사번"))
                            {
                                orgMent = orgMent.Replace("사번", "").Replace(" ", "");
                                isNumeric = int.TryParse(orgMent, out n);
                            }
                            

                            DButil.HistoryLog("***** loginStatus : " + loginStatus + "| isNumeric : " + isNumeric);
                            
                            if (isNumeric)
                            {
                                DButil.HistoryLog("*** loginStatus : "+loginStatus+" | activity.ChannelId : " + activity.ChannelId + " | activity.From.Id : " + activity.From.Id);
                                DButil.HistoryLog("*** orgMent : " + orgMent);
                                Debug.WriteLine("loginStatus===="+ loginStatus);
                                hrList = db.SelectHrInfo(orgMent);
                                //DButil.HistoryLog("*** SelectHrInfo - tmn_cod : " + hrList[0].tmn_cod);
                                if (hrList != null)
                                {
                                    //DButil.HistoryLog("*** SelectHrInfo - tmn_cod : " + hrList[0].tmn_cod);
                                    if (hrList.Count > 0 && hrList[0].user_nm != null)
                                    {
                                        DButil.HistoryLog("*** SELECT hrList : Exist | name : " + hrList[0].user_nm);

                                        userData.SetProperty<string>("loginStatus", "Y");
                                        userData.SetProperty<string>("emp_no", hrList[0].emp_no);
                                        userData.SetProperty<string>("user_nm", hrList[0].user_nm);
                                        userData.SetProperty<string>("dept_cd", hrList[0].dept_cd);
                                        userData.SetProperty<string>("dept_nm", hrList[0].dept_nm);
                                        userData.SetProperty<string>("user_id", hrList[0].user_id);

                                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                        fullentity = "login,ok";
                                        DButil.HistoryLog("*** fullentity : " + fullentity);
                                    } 
                                    else
                                    {
                                        DButil.HistoryLog("*** SELECT hrList : NOT Exist");
                                        userData.SetProperty<string>("loginStatus", "N");
                                        userData.SetProperty<string>("emp_no", "");
                                        userData.SetProperty<string>("user_nm", "");
                                        userData.SetProperty<string>("dept_cd", "");
                                        userData.SetProperty<string>("dept_nm", "");
                                        userData.SetProperty<string>("user_id", "");

                                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                        fullentity = "fail,login";
                                        DButil.HistoryLog("*** fullentity : " + fullentity);
                                    }
                                }
                                else
                                {
                                    DButil.HistoryLog("*** SelectHrInfo : NULL");
                                    //  조회후 사원 번호 존재하지 않을 경우..  
                                    userData.SetProperty<string>("loginStatus", "N");
                                    userData.SetProperty<string>("emp_no", "");
                                    userData.SetProperty<string>("user_nm", "");
                                    userData.SetProperty<string>("dept_cd", "");
                                    userData.SetProperty<string>("dept_nm", "");
                                    userData.SetProperty<string>("user_id", "");

                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                    fullentity = "fail,login";
                                    DButil.HistoryLog("*** loginStatus : N | name : " + userData.GetProperty<string>("name") + " | workerid : " + userData.GetProperty<string>("workerid"));                                    
                                    DButil.HistoryLog("*** fullentity : " + fullentity);
                                }
                            }
                        }


                        //로그인 설정
                        Debug.WriteLine("loginStatus=====" + userData.GetProperty<string>("loginStatus"));
                        if (userData.GetProperty<string>("loginStatus") == "")
                        {
                            Activity reply_ment = activity.CreateReply();
                            reply_ment.Recipient = activity.From;
                            reply_ment.Type = "message";
                            reply_ment.Text = "사번을 입력해주세요.";

                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                            response = Request.CreateResponse(HttpStatusCode.OK);
                            return response;
                        }

                        DButil.HistoryLog("fullentity : " + fullentity);
                        if (!string.IsNullOrEmpty(fullentity) || !fullentity.Equals(""))
                        {
                            
                            //  fullentity 있는 경우..
                            if (!String.IsNullOrEmpty(luisEntities))
                            {
                                DButil.HistoryLog("fullentity : " + fullentity + " | luisEntities : " + luisEntities + "| luisIntent : Y " + luisIntent);
                                //entity 길이 비교
                                if (fullentity.Length > luisEntities.Length || luisIntent == null || luisIntent.Equals(""))
                                {
                                    //DButil.HistoryLog("fullentity : " + fullentity + " | luisEntities : " + luisEntities);
                                    //DefineTypeChkSpare에서는 인텐트나 루이스아이디조건 없이 엔티티만 일치하면 다이얼로그 리턴
                                    relationList = db.DefineTypeChkSpare(fullentity);
                                    //relationList = db.DefineTypeChkSpare(luisIntent);
                                }
                                else
                                {
                                    //DButil.HistoryLog("luisIntent : " + luisIntent);
                                    relationList = db.DefineTypeChk(MessagesController.luisId, MessagesController.luisIntent, MessagesController.luisEntities);
                                }
                            }
                            else
                            {
                                DButil.HistoryLog("fullentity : " + fullentity + " | luisEntities : " + luisEntities + "| luisIntent : N " + luisIntent);
                                relationList = db.DefineTypeChkSpare(fullentity);
                                //relationList = db.DefineTypeChkSpare(luisIntent);
                            }
                            
                            //DButil.HistoryLog("luisid : " + luisId + " | luisintent : " + luisIntent + "| luisEntities : " + luisEntities);
                            //relationList = db.DefineTypeChk(luisId, luisIntent, luisEntities);
                        }
                        else
                        {
                            DButil.HistoryLog("fullentity is NULL | apiFlag : "+ apiFlag + " | cacheList.luisEntities : "+ cacheList.luisEntities);
                            if (apiFlag.Equals("COMMON"))
                            {
                                //relationList = db.DefineTypeChkSpare(cacheList.luisEntities);
                                relationList = null;
                            }
                            else
                            {
                                relationList = null;
                            }
                            //DButil.HistoryLog("fullentity is NULL | relationList : " + relationList);
                        }

                        if (relationList != null)
                        {
                            DButil.HistoryLog("* relationList is NOT NULL !");
                            if (relationList.Count > 0 && relationList[0].dlgApiDefine != null)
                            {
                                if (relationList[0].dlgApiDefine.Equals("api testdrive"))
                                {
                                    apiFlag = "TESTDRIVE";
                                }
                                else if (relationList[0].dlgApiDefine.Equals("api quot"))
                                {
                                    apiFlag = "QUOT";
                                }
                                else if (relationList[0].dlgApiDefine.Equals("api recommend"))
                                {
                                    apiFlag = "RECOMMEND";
                                }
                                else if (relationList[0].dlgApiDefine.Equals("D"))
                                {
                                    apiFlag = "COMMON";
                                }
                                DButil.HistoryLog("relationList[0].dlgApiDefine : " + relationList[0].dlgApiDefine);
                            }
                            else
                            {
                                DButil.HistoryLog("* relationList is NOT NULL ! && relationList.Count : 0 | apiFlag : "+ apiFlag);
                            }

                        }
                        else
                        {
                            DButil.HistoryLog("* relationList is NULL");
                            if (MessagesController.cacheList.luisIntent == null || apiFlag.Equals("COMMON"))
                            {
                                apiFlag = "";
                            }
                            else if (MessagesController.cacheList.luisId.Equals("kona_luis_01") && MessagesController.cacheList.luisIntent.Contains("quot"))
                            {
                                apiFlag = "QUOT";
                            }
                            //DButil.HistoryLog("apiFlag : "+ apiFlag);
                        }

                        if (apiFlag.Equals("COMMON") && relationList.Count > 0)
                        {
                            DButil.HistoryLog("apiFlag : COMMON | relationList.Count : " + relationList.Count);
                            //context.Call(new CommonDialog("", MessagesController.queryStr), this.ResumeAfterOptionDialog);
                            String beforeMent = "";
                            facebookpagecount = 1;
                            //int fbLeftCardCnt = 0;

                            if (conversationhistory.commonBeforeQustion != null && conversationhistory.commonBeforeQustion != "")
                            {
                                DButil.HistoryLog(fbLeftCardCnt + "{fbLeftCardCnt} :: conversationhistory.commonBeforeQustion : " + conversationhistory.commonBeforeQustion);
                                if (conversationhistory.commonBeforeQustion.Equals(orgMent) && activity.ChannelId.Equals("facebook") && fbLeftCardCnt > 0)
                                {
                                    DButil.HistoryLog("beforeMent : " + beforeMent);
                                    conversationhistory.facebookPageCount++;
                                }
                                else
                                {
                                    conversationhistory.facebookPageCount = 0;
                                    fbLeftCardCnt = 0;
                                }
                            }


                            DButil.HistoryLog("* MessagesController.relationList.Count : " + MessagesController.relationList.Count);
                            for (int m = 0; m < MessagesController.relationList.Count; m++)
                            {
                                DialogList dlg = db.SelectDialog(MessagesController.relationList[m].dlgId);
                                Debug.WriteLine("MessagesController.relationList[m].dlgId ==== " + MessagesController.relationList[m].dlgId);
                                Activity commonReply = activity.CreateReply();
                                Attachment tempAttachment = new Attachment();
                                DButil.HistoryLog("dlg.dlgType : " + dlg.dlgType);
                                if (dlg.dlgType.Equals(CARDDLG))
                                {
                                    foreach (CardList tempcard in dlg.dialogCard)
                                    {
                                        DButil.HistoryLog("tempcard.card_order_no : " + tempcard.card_order_no);
                                        if (conversationhistory.facebookPageCount > 0)
                                        {
                                            if (tempcard.card_order_no > (MAXFACEBOOKCARDS * facebookpagecount) && tempcard.card_order_no <= (MAXFACEBOOKCARDS * (facebookpagecount + 1)))
                                            {
                                                tempAttachment = dbutil.getAttachmentFromDialog(tempcard, activity);
                                            }
                                            else if (tempcard.card_order_no > (MAXFACEBOOKCARDS * (facebookpagecount + 1)))
                                            {
                                                fbLeftCardCnt++;
                                                tempAttachment = null;
                                            }
                                            else
                                            {
                                                fbLeftCardCnt = 0;
                                                tempAttachment = null;
                                            }
                                        }
                                        else if (activity.ChannelId.Equals("facebook"))
                                        {
                                            DButil.HistoryLog("facebook tempcard.card_order_no : " + tempcard.card_order_no);
                                            if (tempcard.card_order_no <= MAXFACEBOOKCARDS && fbLeftCardCnt == 0)
                                            {
                                                tempAttachment = dbutil.getAttachmentFromDialog(tempcard, activity);
                                            }
                                            else
                                            {
                                                fbLeftCardCnt++;
                                                tempAttachment = null;
                                            }
                                        }
                                        else
                                        {
                                            //  userLoginOk
                                            if (tempcard.cardTitle.Equals("LoginSuccess")) //  주문내역 dialog 일시..
                                            {
                                                DButil.HistoryLog("*** activity.Conversation.Id : " + activity.Conversation.Id + " | dlg.cardText : " + dlg.cardText + " | fullentity : " + fullentity);

                                                string[] strComment = new string[4];
                                                string optionComment = tempcard.cardText;
                                                

                                                strComment[1] = userData.GetProperty<string>("dept_nm");
                                                strComment[2] = userData.GetProperty<string>("user_nm");
                                                strComment[3] = userData.GetProperty<string>("emp_no");
                                                DButil.HistoryLog("*** strComment[0] : " + strComment[0] + " | strComment[1] : " + strComment[1] + " | strComment[2] : " + strComment[2]);
                                                //B2B영업1팀 SA(11112222) 님.어떤 업무를 도와드릴까요 ?;
                                                optionComment = optionComment.Replace("#Dept_nm", strComment[1]);
                                                optionComment = optionComment.Replace("#User_nm", strComment[2]);
                                                optionComment = optionComment.Replace("#Emp_no", strComment[3]);
                                                optionComment = optionComment.Replace(". ", ".\n\n");
                                                tempcard.cardText = optionComment;
                                            }

                                            if (tempcard.cardTitle.Equals("주문완료")) //  주문내역 dialog 일시..
                                            {
                                                string informV = userData.GetProperty<string>("inform");

                                                if (string.IsNullOrEmpty(inform))
                                                {
                                                    informV = "";
                                                }

                                                if (selectYn == "Y")
                                                {
                                                    DButil.HistoryLog(" selectYn 11111");
                                                    int dbResult1 = db.updateOrder(vbeln, userData.GetProperty<string>("cust"), userData.GetProperty<string>("kunnr"), userData.GetProperty<string>("matnr"), userData.GetProperty<string>("kwmenge"), userData.GetProperty<string>("vdatu"), informV);
                                                    DButil.HistoryLog(" selectYn 22222");
                                                }
                                                else
                                                {
                                                    DButil.HistoryLog(" 주문완료 11111");
                                                    int dbResult1 = db.insertOrder(userData.GetProperty<string>("cust"), userData.GetProperty<string>("kunnr"), userData.GetProperty<string>("matnr"), userData.GetProperty<string>("kwmenge"), userData.GetProperty<string>("vdatu"), informV, userData.GetProperty<string>("emp_no"));
                                                    DButil.HistoryLog(" 주문완료 2222");
                                                }

                                                userData.SetProperty<string>("cust", "");
                                                userData.SetProperty<string>("kunnr", "");
                                                userData.SetProperty<string>("matnr", "");
                                                userData.SetProperty<string>("kwmenge", "");
                                                userData.SetProperty<string>("vdatu", "");
                                                userData.SetProperty<string>("inform", "");
                                                userData.SetProperty<string>("rc", "");
                                                cust = "";
                                                kunnr = "";
                                                matnr = "";
                                                kwmenge = "";
                                                vdatu = "";
                                                inform = "";

                                                ttsCnt = 0;
                                            }
                                            
                                            tempAttachment = dbutil.getAttachmentFromDialog(tempcard, activity);
                                        }



                                        if (tempAttachment != null)
                                        {
                                            commonReply.Attachments.Add(tempAttachment);
                                        }
                                    }
                                }
                                else
                                {
                                    //DButil.HistoryLog("* facebook dlg.dlgId : " + dlg.dlgId);
                                    DButil.HistoryLog("* activity.ChannelId : " + activity.ChannelId);
                                    DButil.HistoryLog("* dlg.dlgId : "+ dlg.dlgId + " | dlg.cardTitle : " + dlg.cardTitle + " | dlg.cardText : " + dlg.cardText);

                                    if (dlg.cardTitle.Equals("주문접수")) //  주문내역 dialog 일시..
                                    {
                                        string informV = userData.GetProperty<string>("inform");

                                        if (string.IsNullOrEmpty(inform))
                                        {
                                            informV = "";
                                        }

                                        userData.SetProperty<string>("cust", "");
                                        userData.SetProperty<string>("kunnr", "");
                                        userData.SetProperty<string>("matnr", "");
                                        userData.SetProperty<string>("kwmenge", "");
                                        userData.SetProperty<string>("vdatu", "");
                                        userData.SetProperty<string>("inform", "");
                                        userData.SetProperty<string>("rc", "");

                                        cust        = "";
                                        kunnr       = "";
                                        matnr       = "";
                                        kwmenge     = "";
                                        vdatu       = "";
                                        inform      = "";
                                        ttsCnt      = 0;
                                    }

                                    //  주문접수
                                    if (dlg.cardTitle.Equals("주문확인")|| dlg.cardTitle.Equals("주문수정거래처")) //  주문내역 dialog 일시..
                                    {
                                        DButil.HistoryLog("=00000");
                                        DButil.HistoryLog("*** activity.Conversation.Id : " + activity.Conversation.Id + " | dlg.cardText : " + dlg.cardText + " | fullentity : " + fullentity);

                                        string[] strComment = new string[5];
                                        string optionComment = dlg.cardText;

                                        //거래처는 해태제과, 인도처는 해태제과 아산공장으로 자재는  갈색설탕 15kg짜리, 수량은 7파레트, 납품일은 6월 1일로  주문넣어줘
                                        DButil.HistoryLog("=11111");
                                        Debug.WriteLine("MessagesController.luisEntitiesVlaue : " + MessagesController.luisEntitiesValue);
                                        DButil.HistoryLog("MessagesController.luisEntitiesVlaue : " + MessagesController.luisEntitiesValue);

                                        //if (selectYn != "Y")
                                        //{
                                        string[] luisEntitiesValueSplit = MessagesController.luisEntitiesValue.Split(',');

                                        for (int i = 0; i < luisEntitiesValueSplit.Count(); i++)
                                        {
                                            if (luisEntitiesValueSplit[i].Contains("거래처내용=") || luisEntitiesValueSplit[i].Contains("거래처코드내용="))
                                            {
                                                    if (luisEntitiesValueSplit[i].Contains("거래처코드내용=")){
                                                        cust = luisEntitiesValueSplit[i].Replace("거래처코드내용=", "").Replace("거래처코드","").Replace("는", "").Replace("은", "");
                                                    }
                                                    else
                                                    {
                                                        cust = luisEntitiesValueSplit[i].Replace("거래처내용=", "");
                                                    }
                                            
                                            }
                                        else if (luisEntitiesValueSplit[i].Contains("납품일자="))
                                        {
                                            vdatu = luisEntitiesValueSplit[i].Replace("납품일자=", "");
                                        }
                                        else if (luisEntitiesValueSplit[i].Contains("수량내용="))
                                        {
                                            kwmenge = luisEntitiesValueSplit[i].Replace("수량내용=", "");
                                        }
                                        else if (luisEntitiesValueSplit[i].Contains("인도처내용=") || luisEntitiesValueSplit[i].Contains("인도처코드내용="))
                                        {
                                            if (luisEntitiesValueSplit[i].Contains("인도처코드내용="))
                                            {
                                                kunnr = luisEntitiesValueSplit[i].Replace("인도처코드내용=", "").Replace("인도처코드", "").Replace("는", "").Replace("은", "");
                                                }
                                            else
                                            {
                                                kunnr = luisEntitiesValueSplit[i].Replace("인도처내용=", "");
                                            }
                                                
                                        }
                                        else if (luisEntitiesValueSplit[i].Contains("자재내용=") || luisEntitiesValueSplit[i].Contains("자재코드내용="))
                                        {
                                            
                                            if (luisEntitiesValueSplit[i].Contains("자재코드내용="))
                                            {
                                                matnr = luisEntitiesValueSplit[i].Replace("자재코드내용=", "").Replace("자재코드", "").Replace("는", "").Replace("은", "");
                                                }
                                            else
                                            {
                                                matnr = luisEntitiesValueSplit[i].Replace("자재내용=", "");
                                            }
                                        }
                                        else if (luisEntitiesValueSplit[i].Contains("전달사항내용="))
                                        {
                                            inform = luisEntitiesValueSplit[i].Replace("전달사항내용=", "");
                                        }
                                        else if (luisEntitiesValueSplit[i].Contains("주문번호내용=")) 
                                            orderNm = luisEntitiesValueSplit[i].Replace("주문번호내용=", "");
                                        }
                                        DButil.HistoryLog("matnr1 : " + matnr); 
                                        if (vdatu.Contains("오늘"))
                                        {
                                            vdatu = DateTime.Now.ToString("yyyy.MM.dd");
                                        }
                                        else if (vdatu.Contains("내일"))
                                        {
                                            vdatu = DateTime.Now.AddDays(1).ToString("yyyy.MM.dd");
                                        }
                                        else if (vdatu.Contains("어제"))
                                        {
                                            vdatu = DateTime.Now.AddDays(-1).ToString("yyyy.MM.dd");
                                        }
                                        else if (vdatu.Contains("다음주"))
                                        {
                                            vdatu = DateTime.Now.AddDays(7).ToString("yyyy.MM.dd");
                                        }

                                        DButil.HistoryLog("=22222");
                                        if (!string.IsNullOrEmpty(cust))
                                        {
                                            if ((cust.Substring(cust.Length - 1)) == ")")
                                            {                                             
                                                cust = cust.Substring(0, cust.Length - (cust.Length - 11));
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(kunnr)) 
                                        {
                                            if ((kunnr.Substring(kunnr.Length - 1)) == ")")
                                            {
                                                kunnr = kunnr.Substring(0, kunnr.Length - (kunnr.Length - 11));
                                            }
                                            
                                        }
                                        DButil.HistoryLog("=333333");
                                        if (string.IsNullOrEmpty(cust))
                                        {
                                            cust = userData.GetProperty<string>("cust");
                                        }

                                        if (string.IsNullOrEmpty(vdatu))
                                        {
                                            vdatu = userData.GetProperty<string>("vdatu");
                                        }

                                        if (string.IsNullOrEmpty(kwmenge))
                                        {
                                            kwmenge = userData.GetProperty<string>("kwmenge");
                                        }

                                        if (string.IsNullOrEmpty(kunnr))
                                        {
                                            kunnr = userData.GetProperty<string>("kunnr");
                                        }
                                        DButil.HistoryLog("matnr2 : " + matnr);
                                        if (string.IsNullOrEmpty(matnr))
                                        {
                                            matnr = userData.GetProperty<string>("matnr");
                                        }
                                        DButil.HistoryLog("matnr3 : " + matnr);
                                        DButil.HistoryLog("=444444");
                                        if(!string.IsNullOrEmpty(vdatu)) { 
                                            if (vdatu.Contains("."))
                                            {
                                                string[] vdatuResult = vdatu.Split('.');
                                                vdatu = vdatuResult[1]+"월" + vdatuResult[2]+"일";
                                            }
                                        }
                                        else
                                        {
                                            vdatu = "";
                                        }

                                        //동일하게, 같은, 똑같고, 변동없고
                                        DButil.HistoryLog("=555555");
                                        if (string.IsNullOrEmpty(kwmenge))
                                        {
                                            DButil.HistoryLog("=in1");
                                            kwmenge = "";
                                            DButil.HistoryLog("=in1");
                                        }
                                        DButil.HistoryLog("=1");
                                        if (string.IsNullOrEmpty(cust))
                                        {
                                            cust = "";
                                        }
                                        if (string.IsNullOrEmpty(kunnr))
                                        {
                                            kunnr = "";
                                        }
                                        DButil.HistoryLog("=2");
                                        if (string.IsNullOrEmpty(matnr))
                                        {
                                            matnr = "";
                                        }
                                        DButil.HistoryLog("=3");
                                        if (kwmenge.Contains("동일하게") || kunnr.Contains("동일하게")  || kunnr.Contains("같은") || kunnr.Contains("똑같고") || kunnr.Contains("변동없고"))
                                        {
                                            kunnr = cust;
                                        }
                                        DButil.HistoryLog("=666666");
                                        List<OrderHistory> orderDlgList = new List<OrderHistory>();
                                        List<PastOrderList> pastOrderList = new List<PastOrderList>();

                                        if (matnr.Contains("기존") || kwmenge.Contains("동일하게") || kwmenge.Contains("같은") || kwmenge.Contains("똑같고") || kwmenge.Contains("변동없고"))
                                        {
                                            pastList = "Y";
                                            DButil.HistoryLog(" SelectPastList 11111");
                                            pastOrderList = db.SelectPastList(cust.Replace(" ", ""), kunnr.Replace(" ", ""), matnr.Replace("기존", "").Replace(" ", ""), userData.GetProperty<string>("emp_no"), vdatu);
                                            DButil.HistoryLog(" SelectPastList 22222");
                                            userData.SetProperty<string>("cust", pastOrderList[0].cust);
                                            userData.SetProperty<string>("kunnr", pastOrderList[0].fixarrival);
                                            userData.SetProperty<string>("matnr", pastOrderList[0].product);
                                            userData.SetProperty<string>("kwmenge", pastOrderList[0].kwmenge);
                                            userData.SetProperty<string>("vdatu", pastOrderList[0].vdatu);

                                            
                                            Activity reply_ment = activity.CreateReply();
                                            if (pastOrderList.Count != 0)
                                            {
                                                for (int i = 0; i < pastOrderList.Count; i++)
                                                {
                                                    cust = pastOrderList[i].cust;
                                                    vdatu = pastOrderList[i].vdatu;
                                                    kwmenge = pastOrderList[i].kwmenge;
                                                    kunnr = pastOrderList[i].fixarrival;
                                                    matnr = pastOrderList[i].product;
                                                    inform = pastOrderList[i].inform;
                                                    vbeln_seq = pastOrderList[i].vbeln_seq;

                                                    if (pastOrderList.Count > 1)
                                                    {
                                                        //인도처: 해태제과(주) 청주공장(119712) 주문 자재 및 수량 : 올리고당 HF25kg(104489) / 200 CAN
                                                        optionComment = (i + 1) + " 주문일 : " + vdatu + "/" + matnr + "/" + kwmenge;

                                                        reply_ment.Recipient = activity.From;
                                                        reply_ment.Type = "message";
                                                        //reply_ment.Text = optionComment;


                                                        var attachment = GetHeroCard(optionComment, vbeln_seq, "자재");
                                                        reply_ment.Attachments.Add(attachment);

                                                    }
                                                    else
                                                    {
                                                        optionComment = "거래처 : " + pastOrderList[0].cust + "\r\n" + "인도처 : " + pastOrderList[0].fixarrival + "\r\n" + "자재 : " + pastOrderList[0].product + "\r\n" + "수량 : " + pastOrderList[0].kwmenge + "\r\n" + "납품일 : " + pastOrderList[0].vdatu;
                                                        if (!string.IsNullOrEmpty(inform))
                                                        {
                                                            optionComment = optionComment + "\r\n" + "전달내용 : " + inform;
                                                            userData.SetProperty<string>("inform", inform);
                                                        }
                                                        dlg.cardText = optionComment;
                                                    }

                                                    selectYn = "Y";

                                                }

                                                if (reply_ment.Attachments.Count != 0)
                                                {
                                                    var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                                                }

                                            }
                                        }
                                        else
                                        {

                                            if (string.IsNullOrEmpty(orderNm))
                                            {
                                                orderNm = "";
                                            }
                                            DButil.HistoryLog(" 주문확인 1111");
                                            DButil.HistoryLog(" cust " + cust);
                                            DButil.HistoryLog(" kunnr " + kunnr);
                                            DButil.HistoryLog(" matnr " + matnr);
                                            DButil.HistoryLog(" kwmenge " + kwmenge);
                                            DButil.HistoryLog(" kwmenge " + vdatu);
                                            DButil.HistoryLog(" orderNm " + orderNm);
                                            DButil.HistoryLog(" inform " + inform);
                                            orderDlgList = db.SelectOrderHistory(cust.Replace(" ", ""), kunnr.Replace(" ", ""), matnr, kwmenge, vdatu, orderNm);
                                            DButil.HistoryLog(" 주문확인 2222");
                                            DButil.HistoryLog(" 주문확인 1111");
                                            DButil.HistoryLog(" orderDlgList[0].cust " + orderDlgList[0].cust);
                                            DButil.HistoryLog(" orderDlgList[0].fixarrival " + orderDlgList[0].fixarrival);
                                            DButil.HistoryLog(" orderDlgList[0].product " + orderDlgList[0].product);
                                            DButil.HistoryLog(" orderDlgList[0].kwmenge " + orderDlgList[0].kwmenge);
                                            DButil.HistoryLog(" orderDlgList[0].vdatu " + orderDlgList[0].vdatu);
                                            userData.SetProperty<string>("cust", orderDlgList[0].cust);
                                            userData.SetProperty<string>("kunnr", orderDlgList[0].fixarrival);
                                            userData.SetProperty<string>("matnr", orderDlgList[0].product);
                                            userData.SetProperty<string>("kwmenge", orderDlgList[0].kwmenge);
                                            userData.SetProperty<string>("vdatu", orderDlgList[0].vdatu);

                                            optionComment = "거래처 : " + orderDlgList[0].cust + "\r\n" + "인도처 : " + orderDlgList[0].fixarrival + "\r\n" + "자재 : " + orderDlgList[0].product + "\r\n" + "수량 : " + orderDlgList[0].kwmenge + "\r\n" + "납품일 : " + orderDlgList[0].vdatu;
                                            //ttsCnt = 0;
                                            //if (!string.IsNullOrEmpty(orderDlgList[0].cust))
                                            //{
                                            //    ttsCnt += 1;
                                            //}
                                            //if (!string.IsNullOrEmpty(orderDlgList[0].fixarrival))
                                            //{
                                            //    ttsCnt += 1;
                                            //}
                                            //if (!string.IsNullOrEmpty(orderDlgList[0].product))
                                            //{
                                            //    ttsCnt += 1;
                                            //}
                                            //if (!string.IsNullOrEmpty(orderDlgList[0].kwmenge))
                                            //{
                                            //    ttsCnt += 1;
                                            //}
                                            //if (!string.IsNullOrEmpty(orderDlgList[0].vdatu))
                                            //{
                                            //    ttsCnt += 1;
                                            //}

                                            DButil.HistoryLog("ttsCnt1 === " + ttsCnt);
                                            if (luisEntitiesValueSplit.Count() > 2)
                                            {
                                                DButil.HistoryLog("ttsCnt2 === " + ttsCnt);
                                                dlg.cardTitle += "_tts";
                                                DButil.HistoryLog("ttsCnt3 === " + ttsCnt);
                                            }
                                            else
                                            {
                                                if (ttsCnt == 0)
                                                {
                                                    dlg.cardTitle += "_tts";
                                                }
                                            }

                                            
                                        }


                                        if (!string.IsNullOrEmpty(inform))
                                        {
                                            optionComment = optionComment + "\r\n" + "전달내용 : " + inform;
                                            userData.SetProperty<string>("inform", inform);
                                        }


                                        dlg.cardText = optionComment;

                                    }

                                    //주문수정수량
                                    if (dlg.cardTitle.Equals("주문수정수량") || dlg.cardTitle.Equals("주문수정납품일") || dlg.cardTitle.Equals("전달사항입력")) //  주문내역 dialog 일시..
                                    {
                                        DButil.HistoryLog("*** activity.Conversation.Id : " + activity.Conversation.Id + " | dlg.cardText : " + dlg.cardText + " | fullentity : " + fullentity);

                                        string optionComment = dlg.cardText;

                                        optionComment = "거래처 : " + userData.GetProperty<string>("cust") + "\r\n" + "인도처 : " + userData.GetProperty<string>("kunnr") + "\r\n" + "자재 : " + userData.GetProperty<string>("matnr") + "\r\n" + "수량 : " + userData.GetProperty<string>("kwmenge") + "\r\n" + "납품일 : " + userData.GetProperty<string>("vdatu");
                                        dlg.cardText = optionComment;

                                    }
                                    
                                    //주문완료
                                    if (dlg.cardTitle.Equals("주문완료")) //  주문내역 dialog 일시..
                                    {
                                        string informV = userData.GetProperty<string>("inform");

                                        if(string.IsNullOrEmpty(inform))
                                        {
                                            informV = "";
                                        }

                                        if (selectYn == "Y")
                                        {
                                            DButil.HistoryLog(" selectYn 11111");
                                            int dbResult1 = db.updateOrder(vbeln, userData.GetProperty<string>("cust"), userData.GetProperty<string>("kunnr"), userData.GetProperty<string>("matnr"), userData.GetProperty<string>("kwmenge"), userData.GetProperty<string>("vdatu"), informV);
                                            DButil.HistoryLog(" selectYn 22222");
                                        }

                                        else
                                        {
                                            DButil.HistoryLog(" 주문완료 11111");
                                            int dbResult1 = db.insertOrder(userData.GetProperty<string>("cust"), userData.GetProperty<string>("kunnr"), userData.GetProperty<string>("matnr"), userData.GetProperty<string>("kwmenge"), userData.GetProperty<string>("vdatu"), informV, userData.GetProperty<string>("emp_no"));
                                            DButil.HistoryLog(" 주문완료 2222");
                                        }                                        

                                        userData.SetProperty<string>("cust", "");
                                        userData.SetProperty<string>("kunnr", "");
                                        userData.SetProperty<string>("matnr", "");
                                        userData.SetProperty<string>("kwmenge", "");
                                        userData.SetProperty<string>("vdatu", "");
                                        userData.SetProperty<string>("inform", "");
                                        userData.SetProperty<string>("rc", "");
                                        cust        = "";
                                        kunnr       = "";
                                        matnr       = "";
                                        kwmenge     = "";
                                        vdatu       = "";
                                        inform      = "";
                                        selectYn    = "";
                                    }

                                    //주문완료
                                    if (dlg.cardTitle.Equals("주문삭제")) //  주문내역 dialog 일시..
                                    {
                                        int dbResult1 = db.deleteOrder(vbeln);

                                        userData.SetProperty<string>("cust", "");
                                        userData.SetProperty<string>("kunnr", "");
                                        userData.SetProperty<string>("matnr", "");
                                        userData.SetProperty<string>("kwmenge", "");
                                        userData.SetProperty<string>("vdatu", "");
                                        userData.SetProperty<string>("inform", "");
                                        userData.SetProperty<string>("rc", "");
                                        cust = "";
                                        kunnr = "";
                                        matnr = "";
                                        kwmenge = "";
                                        vdatu = "";
                                        inform = "";
                                    }

                                    //주문조회거래처납품일
                                    if (dlg.cardTitle.Equals("주문조회거래처납품일"))
                                    {
                                        string[] luisEntitiesValueSplit = MessagesController.luisEntitiesValue.Split(',');

                                        for (int i = 0; i < luisEntitiesValueSplit.Count(); i++)
                                        {
                                            //if (luisEntitiesValueSplit[i].Contains("거래처내용="))
                                            //{                                                
                                            //    cust = luisEntitiesValueSplit[i].Replace("거래처내용=", "").Replace("거래처코드", "").Replace("는", "").Replace("은", "");
                                            //    int n = 0;
                                            //    var isNumeric = int.TryParse(Right(cust, 6), out n);
                                                
                                            //    if (isNumeric)
                                            //    {
                                            //        cust = cust.Substring(0,cust.Length-6);
                                            //    }
                                            //}

                                            if (luisEntitiesValueSplit[i].Contains("거래처내용=") || luisEntitiesValueSplit[i].Contains("거래처코드내용="))
                                            {
                                                if (luisEntitiesValueSplit[i].Contains("거래처코드내용="))
                                                {
                                                    cust = luisEntitiesValueSplit[i].Replace("거래처코드내용=", "").Replace("거래처코드", "").Replace("는", "").Replace("은", "");
                                                }
                                                else
                                                {
                                                    cust = luisEntitiesValueSplit[i].Replace("거래처내용=", "").Replace("코드","");
                                                }

                                            }
                                            else if (luisEntitiesValueSplit[i].Contains("납품일자="))
                                            {
                                                vdatu = luisEntitiesValueSplit[i].Replace("납품일자=", "");
                                            }

                                            
                                        }
                                        dlg.cardTitle += "_tts";
                                        List<OrderList> orderList = db.SelectOrderList(cust, vdatu, userData.GetProperty<string>("emp_no"));
                                        Activity reply_ment = activity.CreateReply();
                                        string optionComment = "";
                                        //예외처리
                                        if (orderList.Count != 0)
                                        {
                                            for (int i = 0; i < orderList.Count; i++)
                                            {
                                                cust        = orderList[i].cust;
                                                vdatu       = orderList[i].vdatu;
                                                kwmenge     = orderList[i].kwmenge;
                                                kunnr       = orderList[i].fixarrival;
                                                matnr       = orderList[i].product;
                                                inform      = orderList[i].inform;
                                                vbeln       = orderList[i].vbeln;
                                                vbeln_seq   = orderList[i].vbeln_seq;
                                                rc          = orderList[i].rc;

                                                if (orderList.Count> 1)
                                                {
                                                    //인도처: 해태제과(주) 청주공장(119712) 주문 자재 및 수량 : 올리고당 HF25kg(104489) / 200 CAN
                                                    optionComment = (i + 1) + "인도처 : " + cust + "주문 자재 수량: " + matnr + "/" + kwmenge;

                                                    reply_ment.Recipient = activity.From;
                                                    reply_ment.Type = "message";
                                                    //reply_ment.Text = optionComment;
                                                    

                                                    var attachment = GetHeroCard(optionComment, vbeln, "주문번호");
                                                    reply_ment.Attachments.Add(attachment);
                                                    

                                                } else
                                                {
                                                    optionComment = "거래처 : " + cust 
                                                                    + "\r\n" + "인도처 : " + kunnr 
                                                                    + "\r\n" + "자재 : " + matnr 
                                                                    + "\r\n" + "수량 : " + kwmenge
                                                                    + "\r\n" + "납품요청일 : " + vdatu;
                                                    if (!string.IsNullOrEmpty(inform))
                                                    {
                                                        optionComment = optionComment + "\r\n" + "전달내용 : " + inform;
                                                        userData.SetProperty<string>("inform", inform);
                                                    }
                                                    dlg.cardText = optionComment;
                                                }
                                                
                                                selectYn = "Y";
                                            }
                                            if (reply_ment.Attachments.Count != 0)
                                            {
                                                var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                                            }
                                            ttsCnt = 1;
                                            
                                        }
                                        else
                                        {
                                            //Activity reply_ment = activity.CreateReply();
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            reply_ment.Text = "일치하는 주문내역이 없어요. 거래처와 주문일자를 다시 말씀해주세요.";

                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                                            response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }

                                        
                                    }

                                    //거래처검색
                                    if (dlg.cardTitle.Equals("거래처검색")) //  주문내역 dialog 일시..
                                    {
                                        List<ClientList> clientList = db.SelectClientList(userData.GetProperty<string>("emp_no"));
                                        Activity reply_ment = activity.CreateReply();
                                        string optionComment = "";
                                        string clientKunnr = "";
                                        string clientKkname1 = "";
                                        if (clientList.Count != 0)
                                        {
                                            for (int i = 0; i < clientList.Count; i++)
                                            {
                                                clientKunnr     = clientList[i].kunnr;
                                                clientKkname1   = clientList[i].kname1;

                                                optionComment = (i + 1) + ". " + clientKkname1 + "(" + clientKunnr + ")";

                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";

                                                var attachment = GetHeroCard(optionComment, clientKkname1 + "(" + clientKunnr + ")", "거래처는 ");
                                                reply_ment.Attachments.Add(attachment);

                                                selectYn = "Y";
                                            }
                                            //kunnr = "";
                                            //kname1 = "";
                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                        }
                                        else
                                        {
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            reply_ment.Text = "거래처를 검색하지 못했어요. 거래처 코드나 이름을 말씀해주세요.";

                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                                            response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }
                                        
                                    }

                                    //인도처검색
                                    if (dlg.cardTitle.Equals("인도처검색")) //  주문내역 dialog 일시..
                                    {
                                        List<ClientList> clientList = db.SelectFixarrivalList();
                                        Activity reply_ment = activity.CreateReply();
                                        string optionComment = "";
                                        string fixarrivalknnr = "";
                                        string fixarrivalkname1 = "";
                                        if (clientList.Count != 0)
                                        {
                                            for (int i = 0; i < clientList.Count; i++)
                                            {
                                                fixarrivalknnr      = clientList[i].kunnr;
                                                fixarrivalkname1    = clientList[i].kname1;

                                                optionComment = (i + 1) + ". " + fixarrivalkname1 + "(" + fixarrivalknnr + ")";

                                                reply_ment.Recipient = activity.From;
                                                reply_ment.Type = "message";

                                                var attachment = GetHeroCard(optionComment, fixarrivalkname1 + "(" + fixarrivalknnr + ")", "인도처는 ");
                                                reply_ment.Attachments.Add(attachment);

                                                selectYn = "Y";
                                            }
                                            //kunnr = "";
                                            //kname1 = "";
                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);

                                        }
                                        else
                                        {
                                            reply_ment.Recipient = activity.From;
                                            reply_ment.Type = "message";
                                            reply_ment.Text = "인도처를 검색하지 못했어요. 인도처 코드나 이름을 말씀해주세요.";

                                            var reply_ment_info = await connector.Conversations.SendToConversationAsync(reply_ment);
                                            response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }
                                    }

                                    //주문번호정보
                                    if (dlg.cardTitle.Equals("주문번호정보"))
                                    {
                                        List<OrderList> orderList = db.SearchOrderList(orderNm);
                                        Activity reply_ment = activity.CreateReply();
                                        string optionComment = "";
                                        //예외처리
                                        if (orderList.Count != 0)
                                        {
                                            for (int i = 0; i < orderList.Count; i++)
                                            {
                                                cust        = orderList[i].cust;
                                                vdatu       = orderList[i].vdatu;
                                                kwmenge     = orderList[i].kwmenge;
                                                kunnr       = orderList[i].fixarrival;
                                                matnr       = orderList[i].product;
                                                inform      = orderList[i].inform;
                                                vbeln       = orderList[i].vbeln;
                                                vbeln_seq   = orderList[i].vbeln_seq;
                                                rc          = orderList[i].rc;
                                                if (i > 0)
                                                {
                                                    if (orderList[i - 1].cust == orderList[i].cust)
                                                    {
                                                        matnr += matnr + "(" + orderList[i].product_nr + ")";
                                                    }
                                                } else
                                                {
                                                    matnr += matnr + "(" + orderList[i].product_nr + ")";
                                                }

                                                optionComment = "거래처 : " + cust + "(" + orderList[i].cust_nr + ")"
                                                                + "\r\n" + "인도처 : " + kunnr + "(" + orderList[i].fixarrival_nr + ")"
                                                                + "\r\n" + "자재 : " + matnr
                                                                + "\r\n" + "수량 : " + kwmenge
                                                                + "\r\n" + "납품요청일 : " + vdatu;
                                                if (!string.IsNullOrEmpty(inform))
                                                {
                                                    optionComment = optionComment + "\r\n" + "전달내용 : " + inform;
                                                    userData.SetProperty<string>("inform", inform);
                                                }
                                                dlg.cardText = optionComment;

                                                selectYn = "Y";
                                            }
                                        }
                                    }

                                    if (activity.ChannelId.Equals("facebook") && string.IsNullOrEmpty(dlg.cardTitle) && dlg.dlgType.Equals(TEXTDLG))
                                    {
                                        commonReply.Recipient = activity.From;
                                        commonReply.Type = "message";
                                        DButil.HistoryLog("facebook card Text : " + dlg.cardText);
                                        commonReply.Text = dlg.cardText;
                                    }
                                    else
                                    {
                                        //if(!dlg.cardTitle.Equals("거래처검색") && !dlg.cardTitle.Equals("인도처검색")) { 
                                        //    tempAttachment = dbutil.getAttachmentFromDialog(dlg, activity);
                                        //    commonReply.Attachments.Add(tempAttachment);
                                        //}
                                        if (!pastList.Equals("Y"))
                                        {
                                            if (!dlg.cardTitle.Equals("거래처검색") && !dlg.cardTitle.Equals("인도처검색"))
                                            {
                                                tempAttachment = dbutil.getAttachmentFromDialog(dlg, activity);
                                                commonReply.Attachments.Add(tempAttachment);
                                                //pastList = "N";
                                            }
                                            
                                        } else
                                        {
                                            reply1.Attachments.Clear();
                                            pastList = "N";
                                        }
                                            

                                        
                                    }

                                }
                                if (commonReply.Attachments.Count > 0)
                                {
                                    SetActivity(commonReply);
                                    conversationhistory.commonBeforeQustion = orgMent;
                                    replyresult = "H";

                                }
                                dlg.cardText = "";
                            }
                        }
                        
                        else
                        {
                            DButil.HistoryLog("* relationList.Count : 0");
                            Debug.WriteLine("* relationList.Count : 0");
                            string newUserID = activity.Conversation.Id;
                            string beforeUserID = "";
                            string beforeMessgaeText = "";
                            //string messgaeText = "";

                            Activity intentNoneReply = activity.CreateReply();
                            Boolean sorryflag = true;


                            if (beforeUserID != newUserID)
                            {
                                beforeUserID = newUserID;
                                MessagesController.sorryMessageCnt = 0;
                            }

                            var message = MessagesController.queryStr;
                            beforeMessgaeText = message.ToString();

                            DButil.HistoryLog("SERARCH MESSAGE : " + message);
                            Debug.WriteLine("SERARCH MESSAGE : " + message);

                            ///
                            queryIntentList = db.SelectQueryIntent(cashOrgMent.Replace(" ", ""));
                            DButil.HistoryLog("*** queryIntentList luisId: " + queryIntentList.luisId + " | luisIntent : " + queryIntentList.luisIntent + " | dlgId : " + queryIntentList.dlgId);
                            Debug.WriteLine("*** queryIntentList luisId: " + queryIntentList.luisId + " | luisIntent : " + queryIntentList.luisIntent + " | dlgId : " + queryIntentList.dlgId);

                            DialogList dlg = db.SelectDialog(queryIntentList.dlgId);
                            Activity commonReply = activity.CreateReply();
                            Attachment tempAttachment = new Attachment();
 
                            Debug.WriteLine("dlg.dlgType : " + dlg.dlgType + " | dlg.cardText : "+ dlg.cardText);
                            intentNoneReply.Attachments = new List<Attachment>();

                            LinkHeroCard card = new LinkHeroCard()
                            {
                                Title = dlg.cardTitle,
                                Subtitle = null,
                                Text = dlg.cardText,
                                Images = null,
                                Buttons = null,
                                //Link = Regex.Replace(serarchList.items[i].link, "amp;", "")
                                Link = null
                            };
                            var attachment = card.ToAttachment();
                            intentNoneReply.Attachments.Add(attachment);

                            if(dlg.cardText != "" && dlg.cardText != null)
                            {
                                Debug.WriteLine("* dlg.cardText : " + dlg.cardText);
                                sorryflag = false;
                                SetActivity(intentNoneReply);
                                replyresult = "S";
                            }
                            else
                            {
                                sorryflag = true;
                            }
                            Debug.WriteLine("* sorryflag : " + sorryflag);
                            
                            
                            if (sorryflag)
                            {
                                Debug.WriteLine("* SORRY Flag True");
                                //Sorry Message 
                                int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(activity.Conversation.Id, MessagesController.chatBotID);

                                //++MessagesController.sorryMessageCnt;

                                Activity sorryReply = activity.CreateReply();

                                sorryReply.Recipient = activity.From;
                                sorryReply.Type = "message";
                                sorryReply.Attachments = new List<Attachment>();
                                sorryReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                                Debug.WriteLine("* SORRY Flag sorryMessageCheck : "+ sorryMessageCheck);
                                List<TextList> text = new List<TextList>();
                                if (sorryMessageCheck == 0)
                                {
                                    text = db.SelectSorryDialogText("5");
                                }
                                else
                                {
                                    text = db.SelectSorryDialogText("6");
                                }
                                Debug.WriteLine("* SORRY Flag True | text : "+text);
                                for (int i = 0; i < text.Count; i++)
                                {
                                    HeroCard plCard = new HeroCard()
                                    {
                                        Title = text[i].cardTitle,
                                        Text = text[i].cardText
                                    };

                                    Attachment plAttachment = plCard.ToAttachment();
                                    sorryReply.Attachments.Add(plAttachment);
                                }

                                SetActivity(sorryReply);
                                //await connector.Conversations.SendToConversationAsync(sorryReply);
                                sorryflag = false;
                                replyresult = "D";
                            }
                        }
                        

                        DateTime endTime = DateTime.Now;
                        //analysis table insert
                        //if (rc != null)
                        //{
                        int dbResult = db.insertUserQuery();

                        //}
                        //history table insert

                        Debug.WriteLine("* insertHistory | Conversation.Id : " + activity.Conversation.Id + "ChannelId : " + activity.ChannelId);

                        db.insertHistory(activity.Conversation.Id, activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds));
                        replyresult = "";
                        recommendResult = "";
                    }
                }
                catch (Exception e)
                {
                    Debug.Print(e.StackTrace);
                    int sorryMessageCheck = db.SelectUserQueryErrorMessageCheck(activity.Conversation.Id, MessagesController.chatBotID);


                    cust = "";
                    kunnr = "";
                    matnr = "";
                    kwmenge = "";
                    vdatu = "";
                    inform = "";
                    selectYn = "";
                    vbeln = "";
                    vbeln_seq = "";
                    rc = "";
                    orderNm = "";
                    pastList = "";

                    ++MessagesController.sorryMessageCnt;

                    Activity sorryReply = activity.CreateReply();

                    sorryReply.Recipient = activity.From;
                    sorryReply.Type = "message";
                    sorryReply.Attachments = new List<Attachment>();
                    //sorryReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    List<TextList> text = new List<TextList>();
                    if (sorryMessageCheck == 0)
                    {
                        text = db.SelectSorryDialogText("5");
                    }
                    else
                    {
                        text = db.SelectSorryDialogText("6");
                    }

                    for (int i = 0; i < text.Count; i++)
                    {
                        HeroCard plCard = new HeroCard()
                        {
                            Title = text[i].cardTitle,
                            Text = text[i].cardText
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        sorryReply.Attachments.Add(plAttachment);
                    }

                    SetActivity(sorryReply);

                    DateTime endTime = DateTime.Now;
                    int dbResult = db.insertUserQuery();
                    Debug.WriteLine("* insertHistory 2");
                    db.insertHistory(activity.Conversation.Id, activity.ChannelId, ((endTime - MessagesController.startTime).Milliseconds));
                    replyresult = "";
                    recommendResult = "";
                }
                finally
                {
                    // facebook 환경에서 text만 있는 멘트를 제외하고 carousel 등록
                    if (!(activity.ChannelId == "facebook" && reply1.Text != ""))
                    {
                        reply1.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    }
                    if (!(activity.ChannelId == "facebook" && reply2.Text != ""))
                    {
                        reply2.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    }
                    if (!(activity.ChannelId == "facebook" && reply3.Text != ""))
                    {
                        reply3.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    }
                    if (!(activity.ChannelId == "facebook" && reply4.Text != ""))
                    {
                        reply4.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    }

                    

                    if (reply1.Attachments.Count != 0 || reply1.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply1);
                    }
                    if (reply2.Attachments.Count != 0 || reply2.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply2);
                    }
                    if (reply3.Attachments.Count != 0 || reply3.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply3);
                    }
                    if (reply4.Attachments.Count != 0 || reply4.Text != "")
                    {
                        await connector.Conversations.SendToConversationAsync(reply4);
                    }

                    //페이스북에서 남은 카드가 있는경우
                    if (activity.ChannelId.Equals("facebook") && fbLeftCardCnt > 0)
                    {
                        Activity replyToFBConversation = activity.CreateReply();
                        replyToFBConversation.Recipient = activity.From;
                        replyToFBConversation.Type = "message";
                        replyToFBConversation.Attachments = new List<Attachment>();
                        replyToFBConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                        replyToFBConversation.Attachments.Add(
                            GetHeroCard_facebookMore(
                            "", "",
                            fbLeftCardCnt + "개의 컨테츠가 더 있습니다.",
                            new CardAction(ActionTypes.ImBack, "더 보기", value: MessagesController.queryStr))
                        );
                        await connector.Conversations.SendToConversationAsync(replyToFBConversation);
                        replyToFBConversation.Attachments.Clear();
                    }
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            response = Request.CreateResponse(HttpStatusCode.OK);
            return response;

        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.Typing)
            {
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            return null;
        }

        private static Attachment GetHeroCard_facebookMore(string title, string subtitle, string text, CardAction cardAction)
        {
            var heroCard = new UserHeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Buttons = new List<CardAction>() { cardAction },
            };
            return heroCard.ToAttachment();
        }

        public async Task<string> DetectLang(string strMsg)
        {
            Debug.WriteLine("***** DetectLang | strMsg : " + strMsg);

            string uri = "https://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + strMsg;
            Debug.WriteLine("***** DetectLang | uri : " + uri);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            //subscriptionKey.Trim()
            httpWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", "81e9d837f9ba43ddb80cf3cb7a67e91f");
            using (WebResponse response = httpWebRequest.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                string languageDetected = (string)dcs.ReadObject(stream);
                Debug.WriteLine(string.Format("Language detected:{0}", languageDetected));
                return languageDetected;
            }
        }

        private static Attachment GetHeroCard(String ment, String str1, String str2)
        {
            string str3 = "";
            string strFull = "";

            //ment : 
            if (str2.Contains("거래처") || str2.Contains("인도처"))
            {
                strFull = str2 + str1 + " 추가해줘.";
                str3 = str1;
            }
            else
            {
                strFull = str2 + " " + str1;
                str3 = str2 + " " + str1;
            }
            var heroCard = new HeroCard
            {
               
                Text = ment,
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, str3, value: strFull) }
            };

            return heroCard.ToAttachment();
        }

        public string remove_html_tag(string html_str)
        {
            return Regex.Replace(html_str, @"[<][a-z|A-Z|/](.|)*?[>]", "");
        }

        public string Right(string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }
    }
}