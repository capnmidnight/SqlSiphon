using System;
using System.Data;
using System.Data.Common;

namespace SqlSiphon.Test.Mock
{
    class MockTransaction : DbTransaction
    {
        public override void Commit()
        {
            throw new NotImplementedException();
        }

        protected override DbConnection DbConnection
        {
            get { throw new NotImplementedException(); }
        }

        public override IsolationLevel IsolationLevel
        {
            get { throw new NotImplementedException(); }
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
