﻿using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public abstract class MofichanIsGreeted : BaseScenario
    {
        protected const string WhenGreetingTemplate = "When I say '<greeting>'";

        protected readonly IList<OutgoingMessage> sentMessages;

        public MofichanIsGreeted(string scenarioTitle) : base(scenarioTitle)
        {
            this.sentMessages = new List<OutgoingMessage>();
            this.MessageSent += (s, e) => this.sentMessages.Add(e.Message);
        }

        protected ExampleTable EmptyExampleTable
        {
            get
            {
                return new ExampleTable("greeting");
            }
        }        

        protected void When_I_say_greeting(string greeting)
        {
            this.When_Mofichan_receives_a_message(sender: this.DeveloperUser, message: greeting);
        }

        protected void Teardown()
        {
            this.sentMessages.Clear();
        }
    }

    public class MofichanIsGreetedWithValidGreeting : MofichanIsGreeted
    {
        private readonly string greetingPattern;

        public MofichanIsGreetedWithValidGreeting() : base(scenarioTitle: "Mofichan is greeted with valid greeting")
        {
            this.greetingPattern = string.Format(@"(?i)(hey|hi|hello),? {0}", this.DeveloperUser.Name);

            var greeting = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_say_greeting(greeting), WhenGreetingTemplate)
                .Then(s => s.Then_Mofichan_should_greet_me_back())
                .WithExamples(Examples)
                .TearDownWith(s => s.TearDown());
        }

        private ExampleTable Examples
        {
            get
            {
                var greetings = from start in new[] { "Hello", "Hey", "Hi", "Sup", "Yo" }
                                from middle in new[] { ",", " " }
                                from name in new[] { "Mofi", "Mofichan" }
                                from end in new[] { "", " ", "!", "." }
                                let greeting = string.Format("{0}{1}{2}{3}", start, middle, name, end)
                                select greeting;

                var table = this.EmptyExampleTable;
                foreach (var greeting in greetings)
                {
                    table.Add(greeting);
                }

                return table;
            }
        }

        private void TearDown()
        {
            this.sentMessages.Clear();
        }

        private void Then_Mofichan_should_greet_me_back()
        {
            var message = this.sentMessages.ShouldHaveSingleItem();
            message.Context.To.ShouldBe(this.DeveloperUser);
            message.Context.Body.ShouldMatch(this.greetingPattern);
        }
    }

    public class MofichanIsGreetedWithInvalidGreeting : MofichanIsGreeted
    {
        public MofichanIsGreetedWithInvalidGreeting() : base(scenarioTitle: "Mofichan is greeted with invalid greeting")
        {
            var greeting = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_say_greeting(greeting), WhenGreetingTemplate)
                .Then(s => s.Then_Mofichan_should_not_have_said_anything())
                .WithExamples(Examples);
        }

        private ExampleTable Examples
        {
            get
            {
                var invalidGreetings = new[]
                {
                    string.Empty,
                    "Foo",
                    "Hello",
                    "Hello Mof",
                    "Goodbye Mofichan",
                    "Mofichan",
                };

                var table = this.EmptyExampleTable;
                foreach (var greeting in invalidGreetings)
                {
                    table.Add(greeting);
                }

                return table;
            }
        }

        private void Then_Mofichan_should_not_have_said_anything()
        {
            this.sentMessages.ShouldBeEmpty();
        }
    }
}
