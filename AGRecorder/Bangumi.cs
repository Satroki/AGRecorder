using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace RadioRecorder
{
    [Serializable]
    public class Bangumi : INotifyPropertyChanged
    {
        private String title = "";
        private String personality = "";
        private Int32 day = 0;
        private Int32 duration = 0;
        private Int32 hour = 0;
        private Int32 minute = 0;

        public String Title
        {
            get { return title; }
            set
            {
                if ((this.title.Equals(value) != true))
                {
                    this.title = value;
                    this.RaisePropertyChanged("Title");
                }
            }
        }
        public String Personality
        {
            get { return personality; }
            set
            {
                if ((this.personality.Equals(value) != true))
                {
                    this.personality = value;
                    this.RaisePropertyChanged("Personality");
                }
            }
        }
        public Int32 Day
        {
            get { return day; }
            set
            {
                if ((this.day.Equals(value) != true))
                {
                    this.day = value;
                    this.RaisePropertyChanged("Time");
                }
            }
        }
        public Int32 Hour
        {
            get { return hour; }
            set
            {
                if ((this.hour.Equals(value) != true))
                {
                    this.hour = value;
                    this.RaisePropertyChanged("Time");
                }
            }
        }
        public Int32 Minute
        {
            get { return minute; }
            set
            {
                if ((this.minute.Equals(value) != true))
                {
                    this.minute = value;
                    this.RaisePropertyChanged("Time");
                }
            }
        }
        public Int32 Duration
        {
            get { return duration; }
            set
            {
                if ((this.duration.Equals(value) != true))
                {
                    this.duration = value;
                    this.RaisePropertyChanged("Duration");
                }
            }
        }
        public String Type { get; set; }

        public String Time { get { return String.Format("{0:D2}:{1:D2}", hour, minute); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    public class RecordTask : Bangumi, IEquatable<RecordTask>
    {
        private String nameFormat = "";
        private String dir = "";
        public String NameFormat
        {
            get { return nameFormat; }
            set
            {
                if ((this.nameFormat.Equals(value) != true))
                {
                    this.nameFormat = value;
                    this.RaisePropertyChanged("NameFormat");
                }
            }
        }
        public String Dir
        {
            get { return dir; }
            set
            {
                if ((this.dir.Equals(value) != true))
                {
                    this.dir = value;
                    this.RaisePropertyChanged("Dir");
                }
            }
        }
        public String GetFileName()
        {
            return Path.Combine(Dir,
                String.Format(NameFormat, DateTime.UtcNow.AddHours(9), this.Title, this.Personality));
        }

        public String GetFileName(Boolean def)
        {
            return Path.Combine(Dir, def
                ? String.Format("{0:yyyyMMdd-}{1:D2}{2:D2}-{3}min.flv", DateTime.UtcNow.AddHours(9), this.Hour, this.Minute, this.Duration)
                : String.Format(NameFormat, DateTime.UtcNow.AddHours(9), this.Title, this.Personality));
        }

        public void SetValue(Bangumi b)
        {
            var props = typeof(Bangumi).GetProperties();
            foreach (var p in props)
            {
                if (p.Name == "Time")
                    continue;
                p.SetValue(this, p.GetValue(b));
            }
        }

        public bool Equals(RecordTask other)
        {
            return this.Day == other.Day && this.Hour == other.Hour && this.Minute == other.Minute;
        }
    }

    [Serializable]
    public class CMDArgs : INotifyPropertyChanged
    {
        private String name = "";
        private String type = "";
        private String args = "";
        private String exe = "";
        public String Exe
        {
            get { return exe; }
            set
            {
                if ((this.exe.Equals(value) != true))
                {
                    this.exe = value;
                    this.RaisePropertyChanged("Exe");
                }
            }
        }
        public String Type
        {
            get { return type; }
            set
            {
                if ((this.type.Equals(value) != true))
                {
                    this.type = value;
                    this.RaisePropertyChanged("DisplayName");
                }
            }
        }
        public String Name
        {
            get { return name; }
            set
            {
                if ((this.name.Equals(value) != true))
                {
                    this.name = value;
                    this.RaisePropertyChanged("DisplayName");
                }
            }
        }
        public String Args
        {
            get { return args; }
            set
            {
                if ((this.args.Equals(value) != true))
                {
                    this.args = value;
                    this.RaisePropertyChanged("Args");
                }
            }
        }
        public String DisplayName { get { return Type + " - " + Name; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class HiBiKiBgm
    {
        public String Type { get; set; }
        public String Title { get; set; }
        public String Count { get; set; }
        public String Date { get; set; }
        public String ImgUri { get; set; }
        public String Tag { get; set; }
        public String Time
        {
            get
            {
                var m = Regex.Match(this.Date, @"\d{1,2}月\d{1,2}日");
                var date = Convert.ToDateTime(m.Value);
                if (date.Month > DateTime.Now.Month)
                    date = date.AddYears(-1);
                return date.ToString("yyMMdd");
            }
        }
    }

    public static class Funcs
    {
        public static String FwToHa(this string a)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in a)
            {
                var i = (int)c;
                if (i >= 0xff10 && i <= 0xff19)
                    sb.Append((char)(i - 0xff10 + 48));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            int index = str.LastIndexOf('.');
            if (index != -1)
                return str.Substring(0, index);
            else
                return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
