namespace s.ad_gp
{
    class LevelingLine
    {
        private string _lineName;  //路线名
        private double _heightDisparity;  //高差
        private double _roadLength;  //道路长度
        private LevelingPoint _startPoint;  //起点
        private LevelingPoint _endPoint;  //终点
        private double _adjustHeightDisparity;  //平差后高差
        private double _u;  //标准化残差
        private bool _error;  //是否存在粗差

        public string LineName { get => _lineName; set => _lineName = value; }
        public double HeightDisparity { get => _heightDisparity; set => _heightDisparity = value; }
        public double RoadLength { get => _roadLength; set => _roadLength = value; }
        internal LevelingPoint StartPoint { get => _startPoint; set => _startPoint = value; }
        internal LevelingPoint EndPoint { get => _endPoint; set => _endPoint = value; }
        public double AdjustHeightDisparity { get => _adjustHeightDisparity; set => _adjustHeightDisparity = value; }
        public double U { get => _u; set => _u = value; }
        public bool Error { get => _error; set => _error = value; }

        public LevelingLine(string lineName, double heightDisparity, double roadLength, LevelingPoint startPoint, LevelingPoint endPoint)
        {
            LineName = lineName;
            HeightDisparity = heightDisparity;
            RoadLength = roadLength;
            StartPoint = startPoint;
            EndPoint = endPoint;
            U = 0;
            Error = false;
        }

        ~LevelingLine()
        {

        }
    }
}
