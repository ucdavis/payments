using Payments.Core.Domain;

namespace payments.Tests.Helpers
{
    public static class CreateValidEntities
    {
        public static Invoice Invoice(int? counter, int lineItemCount = 3, bool loadAll = false)
        {
            var rtValue = new Invoice();
            rtValue.Id = counter ?? 99;
            for (int i = 0; i < lineItemCount; i++)
            {
                rtValue.Items.Add(CreateValidEntities.LineItem(i+1));
            }
            rtValue.Team = new Team();
            rtValue.Team.Slug = "testSlug";

            rtValue.CustomerEmail = $"test{counter}@test.com";
            rtValue.CustomerName = $"CustomerName{counter}";
            if (loadAll)
            {
                //Do rest
            }


            return rtValue;
        }

        public static LineItem LineItem(int? counter)
        {
            var rtValue = new LineItem();
            rtValue.Id = counter ?? 99;
            rtValue.Amount = (counter ?? 99);
            rtValue.Quantity = counter ?? 1;
            rtValue.Total = rtValue.Amount * rtValue.Quantity;
            rtValue.Description = $"Description{counter}";

            return rtValue;
        }

        public static User User(int? counter)
        {
            var rtValue = new User();
            rtValue.FirstName = string.Format("FirstName{0}", counter);
            rtValue.LastName = string.Format("LastName{0}", counter);
            rtValue.Name = string.Format("{0} {1}", rtValue.FirstName, rtValue.LastName);
            rtValue.Email = $"test{counter}@testy.com";



            rtValue.Id = (counter ?? 99).ToString();

            return rtValue;
        }
    }
}