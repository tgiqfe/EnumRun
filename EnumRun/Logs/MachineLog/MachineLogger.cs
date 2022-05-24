using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using EnumRun.Lib;
using EnumRun.Lib.Syslog;

namespace EnumRun.Logs.MachineLog
{
    internal class MachineLogger : LoggerBase
    {
        private ILiteCollection<MachineLogBody> _logstashCollection = null;
        private ILiteCollection<MachineLogBody> _syslogCollection = null;
        private ILiteCollection<MachineLogBody> _dynamicLogCollection = null;

        public MachineLogger(EnumRunSetting setting)
        {
            string logFileName =
                $"MachineLog_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _lock = new AsyncLock();

            if (!string.IsNullOrEmpty(setting.Logstash?.Server))
            {
                _logstash = new LogstashTransport(setting.Logstash.Server);
            }
            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new SyslogTransport(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = MachineLogBody.TAG;
            }
        }

        public void Write()
        {
            SendAsync(new MachineLogBody()).ConfigureAwait(false);
        }

        private async Task SendAsync(MachineLogBody body)
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
                            _logstashCollection ??= GetCollection<MachineLogBody>(MachineLogBody.TAG + "_logstash");
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
                            _syslogCollection ??= GetCollection<MachineLogBody>(MachineLogBody.TAG + "_syslog");
                            _syslogCollection.Upsert(body);
                        }
                    }

                    //  DynamicLog転送
                    if (_dynamicLog != null)
                    {
                        if (_dynamicLog.Enabled)
                        {
                            await _dynamicLog.SendAsync("MachineLog", json);
                        }
                        else
                        {
                            _liteDB ??= GetLiteDB();
                            _dynamicLogCollection ??= GetCollection<MachineLogBody>(MachineLogBody.TAG + "_dynamicLog");
                            _dynamicLogCollection.Upsert(body);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
