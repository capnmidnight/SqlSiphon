namespace SqlSiphon
{
    public delegate void DataProgressEventHandler(object sender, DataProgressEventArgs e);

    public class DataProgressEventArgs
    {
        public int RowCount { get; private set; }
        public int CurrentRow { get; private set; }
        public string Message { get; private set; }

        public DataProgressEventArgs(int currentRow, int rowCount, string message)
        {
            RowCount = rowCount;
            CurrentRow = currentRow;
            Message = message;
        }

        public int GetProgressScale(int scale)
        {
            return CurrentRow * scale / RowCount;
        }
    }
}
