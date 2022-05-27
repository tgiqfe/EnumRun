﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using EnumRun.Lib;
using EnumRun.Lib.Syslog;

namespace EnumRun.Logs.SessionLog
{
    internal class SessionLogger : LoggerBase
    {
        protected override bool _logAppend { get { return true; } }

        private ILiteCollection<SessionLogBody> _logstashCollection = null;
        private ILiteCollection<SessionLogBody> _syslogCollection = null;
        private ILiteCollection<SessionLogBody> _dynamicLogCollection = null;

        public SessionLogger(EnumRunSetting setting, EnumRun.ScriptDelivery.ScriptDeliverySession session)
        {
            string logFileName =
                $"SessionLog_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _lock = new AsyncLock();

            if (!string.IsNullOrEmpty(setting.Logstash?.Server))
            {
                _logstash = new TransportLogstash(setting.Logstash.Server);
            }
            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new TransportSyslog(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = SessionLogBody.TAG;
            }
            if (session.EnableLogTransport)
            {
                _dynamicLog = new TransportDynamicLog(session, "SessionLog");
            }
        }

        public void Write()
        {
            SendAsync(new SessionLogBody()).ConfigureAwait(false);
        }

        public void Write(SessionLogBody body)
        {
            SendAsync(body).ConfigureAwait(false);
        }

        private async Task SendAsync(SessionLogBody body)
        {
            try
            {
                using (await _lock.LockAsync())
                {
                    string json = body.GetJson();

                    //ファイル書き込み
                    await _writer.WriteLineAsync(json);

                    //  Logstash転送
                    if (_logstash != null)
                    {
                        bool res = false;
                        if (_logstash.Enabled)
                        {
                            res = await _logstash.SendAsync(json);
                        }
                        if (!res)
                        {
                            _liteDB ??= GetLiteDB();
                            _logstashCollection ??= GetCollection<SessionLogBody>(SessionLogBody.TAG + "_logstash");
                            _logstashCollection.Upsert(body);
                        }
                    }

                    //  Syslog転送
                    if (_syslog != null)
                    {
                        if (_syslog.Enabled)
                        {
                            foreach (var pair in body.GetSyslogMessage())
                            {
                                await _syslog.SendAsync(LogLevel.Info, pair.Key, pair.Value);
                            }
                        }
                        else
                        {
                            _liteDB ??= GetLiteDB();
                            _syslogCollection ??= GetCollection<SessionLogBody>(SessionLogBody.TAG + "_syslog");
                            _syslogCollection.Upsert(body);
                        }
                    }

                    //  DynamicLog転送
                    if (_dynamicLog != null)
                    {
                        if (_dynamicLog.Enabled)
                        {
                            await _dynamicLog.SendAsync(json);
                        }
                        else
                        {
                            _liteDB ??= GetLiteDB();
                            _dynamicLogCollection ??= GetCollection<SessionLogBody>(SessionLogBody.TAG + "_dynamicLog");
                            _dynamicLogCollection.Upsert(body);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
