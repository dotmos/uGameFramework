using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UniRx;

namespace Service.LoggingService {
    public class LogData
    {
        /// <summary>
        /// The debug message itself
        /// </summary>
        public string message;
        /// <summary>
        /// The type (info,warn,error,severe)
        /// </summary>
        public DebugType type;
        /// <summary>
        /// Domain for optional filtering of logging-data (can be null)
        /// </summary>
        public string domain;

        /// <summary>
        /// The timestamp when the log was created
        /// </summary>
        public DateTime timestamp;

        /// <summary>
        /// The framenumber when this message was sent
        /// </summary>
        public int frameNr; 

        public LogData() {
            timestamp = DateTime.Now;
            //TODO: once we have a time-service or similar
            // frameNr = ....
        }

        public override string ToString() {
            return "[" + type + "] [" + timestamp.ToShortTimeString() + "] "+(domain==""?"": " [domain:" + domain + "] " ) + message;
        }
    }

    public class LoggingFilter : IDisposable
    {
        /// <summary>
        /// Regular expression applied to the log's domain
        /// </summary>
        public ReactiveProperty<string> domainRegExProperty = new ReactiveProperty<string>();

        public string DomainRegEx {
            get { return domainRegExProperty.Value; }
            set { domainRegExProperty.Value = value; }
        }

        public Regex regexDomain = null;

        /// <summary>
        /// Regular expression applied to the log's domain
        /// </summary>
        public ReactiveProperty<string> messageRegexProperty = new ReactiveProperty<string>();

        public string MessageRegEx {
            get { return messageRegexProperty.Value; }
            set { messageRegexProperty.Value = value; }
        }

        public Regex regexMessage = null;

        /// <summary>
        /// Types specified in this list are NOT shown
        /// </summary>
        /// 
        public ReactiveCollection<DebugType> typesExcluded = new ReactiveCollection<DebugType>();


        private CompositeDisposable disposables = new CompositeDisposable();

        public LoggingFilter() {
            domainRegExProperty.Subscribe(newRegEx => {
                if (newRegEx == null) {
                    regexDomain = null;
                } else {
                    regexDomain = new Regex(newRegEx, RegexOptions.IgnoreCase);
                }
            }).AddTo(disposables);
            messageRegexProperty.Subscribe(newRegEx => {
                if (newRegEx == null) {
                    regexMessage = null;
                } else {
                    regexMessage = new Regex(newRegEx, RegexOptions.IgnoreCase);
                }
            }).AddTo(disposables);

            disposables.Add(messageRegexProperty);
            disposables.Add(domainRegExProperty);
        }

        /// <summary>
        /// Get informed when any value got changed
        /// </summary>
        /// <returns></returns>
        public IObservable<bool> rxObserveChange() {
            return Observable.Merge(domainRegExProperty.Skip(1).Select(_ => true),
                                     typesExcluded.ObserveCountChanged().Select(_ => true));
        }

        public bool Check(LogData log) {
            if (regexDomain != null) {
                var regexResult = regexDomain.Match(log.domain).Success;
                if (regexResult == false) {
                    // if we have a domain regex then it must match. if not the logdata is not valid
                    return false;
                }
            }

            if (regexMessage != null) {
                var regexMessageResult = regexMessage.Match(log.message).Success;
                if (regexMessageResult == false) {
                    // if we have a message regex then it must match. if not the logdata is not valid
                    return false;
                }
            }

            if (typesExcluded.Contains(log.type)) {
                return false;
            }

            return true;
        }


        public void Dispose() {
            disposables.Dispose();
        }
    }
}