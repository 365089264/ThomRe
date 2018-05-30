using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.RFA.Common;
using ThomsonReuters.RFA.Config;
using ThomsonReuters.RFA.Data;
using ThomsonReuters.RFA.Message;
using ThomsonReuters.RFA.RDM;
using ThomsonReuters.RFA.SessionLayer;
using CNE.Scheduler.Extension.Model;
using CNEToolsEntities;

namespace CNE.Scheduler.Extension
{
    sealed class StarterConsumer : Client
    {

        private RDMFieldDictionary _rdmFieldDictionary;
        private readonly StringBuilder _rfaLog = new StringBuilder();
        private List<QueueMessageFromRFA> _queueMessageFromRfas;
        private Session _session ;
        private OMMConsumer _ommConsumer ;
        private EventQueue _eventQueue ;
        private long _loginHandle ;

        private ConfigDatabase _configDb ;
        private StagingConfigDatabase _stgConfigDb;

        private List<RmdsFid> _fids ;
        private List<RmdsRic> _rics ;
        private List<string> _currentFids ;
        private string _requestRic ;
        private string _responseRic ;
        //private Dictionary<string, List<RmdsFid>> mapList = new Dictionary<string, List<RmdsFid>>();


        public JobStatus Run(StringBuilder log)
        {
            // ==========================================================================
            _fids = DataTableSerializer.ToList<RmdsFid>(OracleHelper.Query("SELECT  RICTYPE, FIDNAME,  TABNAME,  COLNAME,COLTYPE FROM FIDLIST").Tables[0]);
            _rics = DataTableSerializer.ToList<RmdsRic>(OracleHelper.Query("SELECT   Ric, RICTYPE FROM RICLIST   ").Tables[0]);
            // NewListBond.GetNewBondRics(_rics);
            CtrlBreakHandler.Init();
            _queueMessageFromRfas = new List<QueueMessageFromRFA>();
            // Initialize Context, Config, and Dictionary
            Context.Initialize();
            _configDb = ConfigDatabase.Acquire(new RFA_String("RFA"));
            _stgConfigDb = StagingConfigDatabase.Create();
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = "ExampleRFA.cfg";
            configFilePath = rootPath + configFilePath;
            _stgConfigDb.Load(ConfigRepositoryTypeEnum.flatFile, new RFA_String(configFilePath));
            _configDb.Merge(_stgConfigDb);
            _rdmFieldDictionary = RDMFieldDictionary.Create();

            string fieldDictPath = "RDMFieldDictionary";
            fieldDictPath = Path.Combine(rootPath, fieldDictPath);
            string enumTypeDefPath = "enumtype.def";
            enumTypeDefPath = Path.Combine(rootPath, enumTypeDefPath);

            _rdmFieldDictionary.ReadRDMFieldDictionary(new RFA_String(fieldDictPath));
            _rdmFieldDictionary.ReadRDMEnumTypeDef(new RFA_String(enumTypeDefPath));
            _rdmFieldDictionary.Version = new RFA_String("1.1");
            _rdmFieldDictionary.DictId = 1;

            // ==========================================================================
            // Initialize EventQueue, Session, and OMMConsumer
            _eventQueue = EventQueue.Create(new RFA_String("StartConsumerEventQueue"));
            _session = Session.Acquire(new RFA_String("Session1"));
            _ommConsumer = _session.CreateOMMConsumer(new RFA_String("StartConsumer"));

            // ==========================================================================
            // Send Login request
            ReqMsg reqMsg = new ReqMsg
            {
                MsgModelType = RDM.MESSAGE_MODEL_TYPES.MMT_LOGIN,
                InteractionType =
                    ReqMsg.InteractionTypeFlag.InitialImage | ReqMsg.InteractionTypeFlag.InterestAfterRefresh
            };
            AttribInfo attribInfo = new AttribInfo
            {
                NameType = Login.USER_ID_TYPES.USER_NAME,
                Name = new RFA_String("trep")
            };
            reqMsg.AttribInfo = attribInfo;
            OMMItemIntSpec ommItemIntSpec = new OMMItemIntSpec {Msg = reqMsg};
            if (_ommConsumer != null)
            {
                _ommConsumer.RegisterClient(_eventQueue, ommItemIntSpec, this);
                //string[] ricTypes = { "CFXS/NEWISSUE", "CNREPO/PBOC", "SLO/PBOC", "SLF/PBOC", "MLF/PBOC" };

                // ==========================================================================
                // Send Market Price item request
                _rfaLog.Append("<ol>");
                foreach (var ric in _rics)
                {
                    _requestRic = ric.Ric;
                    _responseRic = null;
                    _currentFids =
                        _fids.Where(re => re.Rictype == ric.Rictype && re.FidName != null)
                            .Select(re => re.FidName)
                            .ToList();

                    reqMsg.Clear();
                    attribInfo.Clear();
                    _eventQueue = EventQueue.Create(new RFA_String("StartConsumerEventQueue"));
                    reqMsg.MsgModelType = RDM.MESSAGE_MODEL_TYPES.MMT_MARKET_PRICE;
                    reqMsg.InteractionType = ReqMsg.InteractionTypeFlag.InitialImage | ReqMsg.InteractionTypeFlag.InterestAfterRefresh;
                    attribInfo.NameType = RDM.INSTRUMENT_NAME_TYPES.INSTRUMENT_NAME_RIC;
                    attribInfo.Name = new RFA_String(ric.Ric);
                    attribInfo.ServiceName = new RFA_String("ELEKTRON_DD");
                    reqMsg.AttribInfo = attribInfo;
                    ommItemIntSpec.Msg = reqMsg;
                    _ommConsumer.RegisterClient(_eventQueue, ommItemIntSpec, this);
                    _rfaLog.Append("<li>Request Ric <b>" + _requestRic+"</b>");
                    Console.WriteLine("Dispatching events for 60 seconds.");
                    System.DateTime currentTime = System.DateTime.Now;
                    System.DateTime startTime = currentTime;
                    System.DateTime endTime = currentTime.AddSeconds(60);
                    while ((!CtrlBreakHandler.IsTerminated()) && (currentTime < endTime || endTime == startTime))
                    {
                        if (_eventQueue != null)
                        {
                            int dispatchReturn = _eventQueue.Dispatch(10);
                            if ((dispatchReturn == Dispatchable.DispatchReturnEnum.NothingDispatchedInActive) ||
                                (dispatchReturn == Dispatchable.DispatchReturnEnum.NothingDispatchedNoActiveEventStreams))
                                break;
                        }
                        currentTime = System.DateTime.Now;
                    }
                    CtrlBreakHandler.SetTerminated(false);
                    if (_responseRic==null)
                    {
                        _rfaLog.Append(" ,<span style=\"color:red;\">No response data</span>");
                    }
                    _rfaLog.Append("</li>");
                }
            }
            log.Append(_rfaLog+"</ol>");

            foreach (var ricType in _queueMessageFromRfas.Select(re=>re.RicType).Distinct())
            {
                log.Append("Table:<b>" + _fids.Where(re => re.Rictype == ricType).Select(re => re.TabName).First() + "</b> RicType:<b>" + ricType + "</b> update:" + _queueMessageFromRfas.Count(re => re.RicType == ricType && re.OperationType == "Update") + " insert:" + _queueMessageFromRfas.Count(re => re.RicType == ricType && re.OperationType == "Insert") + " ignore:" + _queueMessageFromRfas.Count(re => re.RicType == ricType && re.OperationType == "Ignore") + "\n");
            }
            if (_queueMessageFromRfas.Count == _rics.Count)
            {
                return JobStatus.Success;
            }
            log.Append("<b><span style=\"color:red;\">Some rics no response data!</span></b>");
            return JobStatus.Fail;
        }


        public void ProcessEvent(Event evnt)
        {
            if (evnt.Type == SessionLayerEventTypeEnum.OMMItemEvent)
            {
                OMMItemEvent ommItemEvent = evnt as OMMItemEvent;
                if (ommItemEvent != null)
                {
                    Msg msg = ommItemEvent.Msg;
                    if (msg.MsgType == MsgTypeEnum.RespMsg)
                    {
                        RespMsg respMsg = msg as RespMsg;
                        if (respMsg != null)
                            switch (respMsg.MsgModelType)
                            {
                                case RDM.MESSAGE_MODEL_TYPES.MMT_LOGIN:
                                    Console.WriteLine("<- Received Login Response");
                                    break;
                                case RDM.MESSAGE_MODEL_TYPES.MMT_MARKET_PRICE:
                                    ProcessMarketPrice(respMsg);
                                    break;
                            }
                    }
                }
            }
        }

        private void ProcessMarketPrice(RespMsg respMsg)
        {
            Console.WriteLine("<- Received Market Price " + respMsg.RespType.ToString());

            // ==========================================================================
            // Display AttribInfo
            if ((respMsg.HintMask & RespMsg.HintMaskFlag.AttribInfo) != 0)
            {
                if ((respMsg.AttribInfo.HintMask & AttribInfo.HintMaskFlag.ServiceName) != 0)
                    Console.WriteLine("Service name: " + respMsg.AttribInfo.ServiceName.ToString());
                if ((respMsg.AttribInfo.HintMask & AttribInfo.HintMaskFlag.Name) != 0)
                    Console.WriteLine("Symbol name: " + respMsg.AttribInfo.Name.ToString());
            }

            // ==========================================================================
            // Decode Payload
            if ((respMsg.HintMask & RespMsg.HintMaskFlag.Payload) != 0)
            {
                _responseRic = respMsg.AttribInfo.Name.ToString();
                Data payload = respMsg.Payload;
                if (payload.DataType == DataEnum.FieldList)
                {
                    FieldList fieldList = payload as FieldList;
                    //Console.WriteLine("FieldList's entry count: " + fieldList.StandardDataCount);
                    var currentRicType =
                        _rics.Where(re => re.Ric == respMsg.AttribInfo.Name.ToString()).Select(re => re.Rictype).First();
                    OpenMarketOperation bs = new OpenMarketOperation(_fids.Where(re => re.Rictype == currentRicType).Select(re => re.TabName).First());
                    bs.setRic(respMsg.AttribInfo.Name.ToString());

                    if (fieldList != null)
                        foreach (FieldEntry fieldEntry in fieldList)
                        {
                            var fieldId = fieldEntry.FieldID;
                            try
                            {

                                RDMFidDef fidDef = _rdmFieldDictionary.GetFidDef(fieldId);
                                Data dataEntry = fieldEntry.GetData(fidDef.OMMType);
                                if (dataEntry.DataType == DataEnum.DataBuffer)
                                {
                                    if (!_currentFids.Contains(fidDef.Name.ToString())) continue;

                                    DataBuffer dataBuffer = dataEntry as DataBuffer;
                                    //Console.Write("\tFieldEntry: {0,-10} {1,-8}\t", fidDef.Name, "(" + fieldId + ")");
                                    if (dataBuffer != null)
                                    {
                                        string fidValue = dataBuffer.GetAsString().ToString();
                                        //Console.WriteLine(fidValue);

                                        var fid =
                                            _fids.First(re => re.Rictype == currentRicType && re.FidName == fidDef.Name);
                                        string colName = fid.ColName;
                                        if (colName != null) bs.getColIdx(colName);
                                        bs.appendColNames(fid.ColName, fid.ColType);
                                        if (fid.ColType == "DATE")
                                        {
                                            fidValue = string.IsNullOrEmpty(fidValue) ? "null" : fidValue.Replace(' ', '-');
                                        }
                                        else if (fid.ColType == "FLOAT")
                                        {
                                            fidValue = string.IsNullOrEmpty(fidValue) ? "0" : float.Parse(fidValue).ToString(CultureInfo.InvariantCulture);
                                        }
                                        else if (fid.ColType == "INT")
                                        {
                                            fidValue = string.IsNullOrEmpty(fidValue) ? "0" : Convert.ToInt32(fidValue).ToString();
                                        }

                                        bs.appendValues(fidValue);
                                    }
                                }
                                if (_responseRic == _requestRic)
                                {
                                    CtrlBreakHandler.SetTerminated(true);
                                }
                            
                            }
                            catch (InvalidUsageException)
                            {
                            }
                        }
                    ExecuteSqlByRicType(currentRicType, bs);
                }
            }
        }

        private void ExecuteSqlByRicType(string rictype,OpenMarketOperation operation)
        {
            QueueMessageFromRFA message = new QueueMessageFromRFA
            {
                Ric = operation._ric,
                RicType = rictype
            };
            switch (rictype)
            {
                case "CFXS/NEWISSUE":
                    NewListBond nb = new NewListBond();
                    nb.Init(operation,  message);
                    break;
                case "CNREPO/PBOC":
                    OpenMarketCNREPO cn = new OpenMarketCNREPO();
                    cn.Init(operation,  message);
                    break;
                case "MLF/PBOC":
                    OpenMarketMLF mlf = new OpenMarketMLF();
                    mlf.Init(operation,  message);
                    break;
                case "SLF/PBOC":
                    OpenMarketSLF slf = new OpenMarketSLF();
                    slf.Init(operation, message);
                    break;
                case "SLO/PBOC":
                    OpenMarketSLO slo = new OpenMarketSLO();
                    slo.Init(operation, message);
                    break;

            }
            _queueMessageFromRfas.Add(message);

            _rfaLog.Append("\n <span style=\"background:" + (_responseRic == _requestRic ? "auto" : "yellow") + ";\">Response ric is <b>" + _responseRic + "</b></span> and " + message.OperationType + message.ReturnMessage + " \n");
               
            if (message.OperationType != "Ignore")
            {
                _rfaLog.Append("Execute sql:" + message.ExecSql + " ; \n");
            }
        }

        public void Cleanup()
        {
            // Clean up
            Console.WriteLine("Cleaning up...");
            if (_eventQueue != null)
            {
                _eventQueue.Deactivate();
            }
            if (_ommConsumer != null)
            {
                if (_loginHandle != 0)
                {
                    _ommConsumer.UnregisterClient(_loginHandle);
                    _loginHandle = 0;
                }
                _ommConsumer.Destroy();
                _ommConsumer = null;
            }
            if (_session != null)
            {
                _session.Release();
                _session = null;
            }
            if (_eventQueue != null)
            {
                _eventQueue.Destroy();
                _eventQueue = null;
            }
            if (_stgConfigDb != null)
            {
                _stgConfigDb.Destroy();
                _stgConfigDb = null;
            }
            if (_configDb != null)
            {
                _configDb.Release();
            }

            if (_rdmFieldDictionary != null)
            {
                _rdmFieldDictionary.Destroy();
            }

            if ((!Context.Uninitialize()) && (Context.InitializedCount == 0))
            {
                Console.WriteLine("RFA Context fails to uninitialize.");
            }

            CtrlBreakHandler.Exit();
        }



    }
}
