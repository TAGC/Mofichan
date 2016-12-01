using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Core.Interfaces;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class UserGetsMofichanAttention : BaseScenario
    {
        public UserGetsMofichanAttention() : base("A user gets Mofichan's attention")
        {
            var call = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("attention"))
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, call))
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(10))
                .Then(s => s.Then_Mofichan_should_let__user__know_they_have_her_attention(this.JohnSmithUser))
                .WithExamples(Examples)
                .TearDownWith(s => s.TearDown());
        }

        private ExampleTable Examples
        {
            get
            {
                var table = new ExampleTable("call");

                table.Add("mofi");
                table.Add("mofichan");
                table.Add("mofi?");
                table.Add("mofi?!");
                table.Add("mofichan!");
                table.Add("mofichan?!");
                table.Add("  mofi   ");
                table.Add(" mofichan  !!");

                return table;
            }
        }

        private void Then_Mofichan_should_let__user__know_they_have_her_attention(IUser user)
        {
            var expectedResponsePatterns = new[] { "hm", "yes[?]", "hi[?]" };

            var validResponses = from response in this.SentMessages
                                 from pattern in expectedResponsePatterns
                                 where Regex.IsMatch(response.Context.Body, pattern)
                                 where response.Context.To == user
                                 select response;

            validResponses.ShouldHaveSingleItem();
        }
    }
}
