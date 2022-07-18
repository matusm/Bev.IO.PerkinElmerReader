namespace Bev.IO.PerkinElmerReader
{
    public class Tupel
    {
        public double X;
        public double Y;
        public bool IsValid => AllComponentsAreValid();

        public Tupel(double x, double y)
        {
            X = x;
            Y = y;
        }

        private bool AllComponentsAreValid()
        {
            if (double.IsNaN(X))
                return false;
            if (double.IsNaN(Y))
                return false;
            return true;
        }

        public override string ToString() => $"[Tupel: X={X}, Y={Y}]";
    }
}
