using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaikoSimilar
{
    class CFumen
    {
        public string strTITLE;
        public string strSUBTITLE;
        public string strWAVE;
        public double dbBPM;
        public double dbOFFSET;
        public double dbDEMOSTART;
        public class CNote
        {
            public enum BranchType
            {
                P,
                R
            }
            public BranchType eBranchType;
            public int nBranchX;
            public int nBranchY;
            public bool bBranchStart;
            public bool bDraw;
            public bool bHit;
            public bool bOver;
            public char chNumber;
            public long lTime;
            public long lHBSTime;
            public long lNowHBSTime;
            public long lNowTime;
            public double dbBPM;
            public double dbSCROLL;
            public double[] dbMEASURE = new double[2];
            public int nBranch;
        }
        public class CCourse
        {
            public int nLEVEL;
            public List<int> BALLOONS = new List<int>();
            public List<CNote> Notes = new List<CNote>();
            public int[] nNoteCount = new int[3];
        }
        public CCourse[] ClCourse = new CCourse[5] { new CCourse(),
        new CCourse(),
        new CCourse(),
        new CCourse(),
        new CCourse()};
        private int StringToCourse(string str)
        {
            switch (str.ToUpper())
            {
                case "EASY":
                case "0":
                    return 0;
                case "NORMAL":
                case "1":
                    return 1;
                case "HARD":
                case "2":
                    return 1;
                case "ONI":
                case "3":
                    return 3;
                case "EDIT":
                case "4":
                    return 4;
                default:
                    return 0;
            }
        }
        public CFumen(string filename)
        {
            using (var sr = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS")))
            {
                int nbranch = 0;
                List<CNote> notes = new List<CNote>();
                bool noteload = false;
                int nowcourse = 0;
                double dbbpm = 0;
                double dbscroll = 0;
                double[] dbmeasure = new double[2];
                long ltime = 0;
                long lhbstime = 0;
                long lbranchtime = 0;
                long lbranchhbstime = 0;
                while (sr.Peek() != -1)
                {
                    var str = sr.ReadLine();
                    int slash = 0;
                    int num = 0;
                    foreach (var ch in str)
                    {
                        if (ch == '/')
                        {
                            slash++;
                        }
                        else
                        {
                            if (slash < 2)
                            {
                                num++;
                                slash = 0;
                            }
                        }
                    }
                    if (slash == 2)
                    str = str.Remove(num, str.Length - num);
                    var strsplit = str.Split(':');
                    if (strsplit.Length > 1)
                    {
                        if (strsplit[0].ToUpper() == "TITLE") strTITLE = strsplit[1];
                        if (strsplit[0].ToUpper() == "SUBTITLE") strSUBTITLE = strsplit[1];
                        if (strsplit[0].ToUpper() == "WAVE") strWAVE = strsplit[1];
                        if (strsplit[0].ToUpper() == "BPM")
                        {
                            if (double.TryParse(strsplit[1], out double db)) dbBPM = db;
                            else dbBPM = 150;
                        }
                        if (strsplit[0].ToUpper() == "OFFSET")
                        {
                            if (double.TryParse(strsplit[1], out double db)) dbOFFSET = db;
                            else dbOFFSET = 150;
                        }
                        if (strsplit[0].ToUpper() == "DEMOSTART")
                        {
                            if (double.TryParse(strsplit[1], out double db)) dbDEMOSTART = db;
                            else dbDEMOSTART = 150;
                        }
                        if (strsplit[0].ToUpper() == "COURSE") nowcourse = StringToCourse(strsplit[1]);
                        if (strsplit[0].ToUpper() == "LEVEL")
                        {
                            if (int.TryParse(strsplit[1], out int n)) ClCourse[nowcourse].nLEVEL = n;
                            else ClCourse[nowcourse].nLEVEL = 0;
                        }
                        if (strsplit[0].ToUpper() == "BALLOON")
                        {
                            foreach (var sp in str.Split(','))
                            {
                                if (int.TryParse(sp, out int n)) ClCourse[nowcourse].BALLOONS.Add(n);
                                else ClCourse[nowcourse].BALLOONS.Add(5);
                            }
                        }
                    }
                    if (str.ToUpper() == "#END")
                    {
                        noteload = false;
                    }
                    if (str.ToUpper().Contains("#BPMCHANGE")) dbbpm = double.Parse(str.ToUpper().Replace("#BPMCHANGE", ""));
                    if (str.ToUpper().Contains("#SCROLL")) dbscroll = double.Parse(str.ToUpper().Replace("#SCROLL", ""));
                    if (str.ToUpper().Contains("#MEASURE"))
                    {
                        var split = str.ToUpper().Replace("#MEASURE", "").Split('/');
                        for (int i = 0; i < 2; i++)
                        {
                            if (double.TryParse(split[i], out double d)) dbmeasure[i] = double.Parse(split[i]);
                            else dbmeasure[i] = 1;
                        }
                    }
                    if (str.ToUpper().Contains("#BRANCHSTART"))
                    {
                        var sp = str.ToUpper().Replace("#BRANCHSTART", "").Split(',');
                        lbranchtime = ltime;
                        lbranchhbstime = lhbstime;
                        CNote cnote = new CNote();
                        cnote.chNumber = '0';
                        cnote.dbBPM = dbbpm;
                        cnote.dbSCROLL = dbscroll;
                        cnote.dbMEASURE[0] = dbmeasure[0];
                        cnote.dbMEASURE[1] = dbmeasure[1];
                        cnote.bDraw = true;
                        cnote.lTime = ltime;
                        cnote.lHBSTime = lhbstime;
                        cnote.bBranchStart = true;
                        if (sp[0].Contains("P"))
                        {
                            cnote.eBranchType = CNote.BranchType.P;
                        }
                        if (sp[0].Contains("R"))
                        {
                            cnote.eBranchType = CNote.BranchType.R;
                        }
                        if (int.TryParse(sp[1], out int n1)) cnote.nBranchX = n1;
                        if (int.TryParse(sp[2], out int n2)) cnote.nBranchY = n2;
                        ClCourse[nowcourse].Notes.Add(cnote);
                    }
                    if (str.ToUpper() == "#BRANCHEND")
                    {
                        nbranch = 0;
                    }
                    if (str.ToUpper() == "#N")
                    {
                        ltime = lbranchtime;
                        lhbstime = lbranchhbstime;
                        nbranch = 0;
                    }
                    if (str.ToUpper() == "#E")
                    {
                        ltime = lbranchtime;
                        lhbstime = lbranchhbstime;
                        nbranch = 1;
                    }
                    if (str.ToUpper() == "#M")
                    {
                        ltime = lbranchtime;
                        lhbstime = lbranchhbstime;
                        nbranch = 2;
                    }
                    if (noteload && !str.Contains("#"))
                    {
                        foreach (var ch in str)
                        {
                            if (ch == '/') break;
                            switch (ch)
                            {
                                case ',':
                                    {
                                        if (notes.Count > 0)
                                        {
                                            foreach (var cnote in notes)
                                            {
                                                cnote.lTime = ltime;
                                                cnote.lHBSTime = lhbstime;
                                                ClCourse[nowcourse].Notes.Add(cnote);
                                                ltime += (long)((240000000 / cnote.dbBPM) * (cnote.dbMEASURE[0] / cnote.dbMEASURE[1])) / notes.Count;
                                                lhbstime += (long)(740000) / notes.Count;
                                            }
                                        }
                                        else
                                        {
                                            CNote cnote = new CNote();
                                            if (nbranch == 0)
                                                cnote.bDraw = true;
                                            cnote.chNumber = ch;
                                            cnote.dbBPM = dbbpm;
                                            cnote.dbSCROLL = dbscroll;
                                            cnote.dbMEASURE[0] = dbmeasure[0];
                                            cnote.dbMEASURE[1] = dbmeasure[1];
                                            cnote.lTime = ltime;
                                            cnote.lHBSTime = lhbstime;
                                            cnote.nBranch = nbranch;
                                            ClCourse[nowcourse].Notes.Add(cnote);
                                            ltime += (long)((240000000 / cnote.dbBPM) * (cnote.dbMEASURE[0] / cnote.dbMEASURE[1]));
                                            lhbstime += (long)(740000);
                                        }
                                        notes.Clear();
                                    }
                                    break;
                                default:
                                    {
                                        if (ch != ' ')
                                        {
                                            CNote cnote = new CNote();
                                            if (nbranch == 0)
                                                cnote.bDraw = true;
                                            cnote.nBranch = nbranch;
                                            cnote.chNumber = ch;
                                            cnote.dbBPM = dbbpm;
                                            cnote.dbSCROLL = dbscroll;
                                            cnote.dbMEASURE[0] = dbmeasure[0];
                                            cnote.dbMEASURE[1] = dbmeasure[1];
                                            notes.Add(cnote);
                                            if (ch == '1' || ch == '2' || ch == '3' || ch == '4')
                                                ClCourse[nowcourse].nNoteCount[nbranch]++;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    if (str.ToUpper() == "#START")
                    {
                        nbranch = 0;
                        dbbpm = dbBPM;
                        dbscroll = 1;
                        dbmeasure[0] = 4;
                        dbmeasure[1] = 4;
                        ltime = 0;
                        lhbstime = 0;
                        noteload = true;
                    }
                }
            }
        }
    }
}
