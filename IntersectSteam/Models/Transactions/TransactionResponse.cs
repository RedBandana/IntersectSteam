
namespace IntersectSteam.Models.Transactions
{
    public class TransactionResponse
    {
        public Response Response { get; set; }

        public TransactionResponse() { }

        public TransactionResponse(Response response) 
        {
            Response = response;
        }
    }
}
