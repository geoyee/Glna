namespace s.ad_gp
{
    class LevelingPoint
    {
        private string _name;  //点名
        private double _height;  //高程
        private double _sigma;  //中误差
        private double _adjustHeight;  //平差后高程
        private double _qv;  //对应的Qii

        public string Name { get => _name; set => _name = value; }
        public double Height { get => _height; set => _height = value; }
        public double Sigma { get => _sigma; set => _sigma = value; }
        public double AdjustHeight { get => _adjustHeight; set => _adjustHeight = value; }
        public double Qv { get => _qv; set => _qv = value; }

        public LevelingPoint(string name, double height)
        {
            Name = name;
            Height = height;
        }

        ~LevelingPoint()
        {

        } 
    }
}
