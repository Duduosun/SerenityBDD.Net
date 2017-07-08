using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SerenityBDD.Core.Extensions;
using SerenityBDD.Core.Time;

namespace SerenityBDD.Core.Model
{
    public class FailureDetails
    {
        private readonly TestOutcome testOutcome;

    public FailureDetails(TestOutcome testOutcome)
        {
            this.testOutcome = testOutcome;
        }

        public string getConciseErrorMessage()
        {
            if (testOutcome.firstStepWithErrorMessage().isPresent())
            {
                return testOutcome.firstStepWithErrorMessage().get().getConciseErrorMessage();
            }
            return testOutcome.TestFailureMessage.Or("");
        }

        public string getCompleteErrorMessage()
        {
            if (testOutcome.firstStepWithErrorMessage().isPresent())
            {
                return testOutcome.firstStepWithErrorMessage().get().getErrorMessage();
            }
            return testOutcome.TestFailureMessage.Or("");
        }

        public string getPageSourceLink()
        {
            foreach (TestStep testStep in testOutcome.getFlattenedTestSteps())
            {
                foreach (var screenshot in testStep.getScreenshots())
                {
                    if (screenshot.getHtmlSourceName() != null)
                    {
                        return screenshot.getHtmlSourceName();
                    }
                }
            }
            return "#";
        }
    }
}
