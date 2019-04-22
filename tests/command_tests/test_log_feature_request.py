import unittest
from commands.log_feature_request import LogFeatureRequestCommand
from config import Config
from model.feature_request import FeatureRequest, FeatureRequestState


class TestLogFeatureRequest(unittest.TestCase):

    def test_persistence(self):
        # Arrange
        Config().connect_to_db()
        requester = "testUser1"
        request = "make something happen"
        request_state = FeatureRequestState.REQUESTED
        fr = FeatureRequest(requester=requester,
                            request=request,
                            request_state=str(request_state))

        # Act
        c = LogFeatureRequestCommand()
        saved_request = c.execute(fr.requester, fr.request)

        # Assert
        self.assertEqual(fr.requester, saved_request.requester)
        self.assertEqual(fr.request, saved_request.request)
        self.assertEqual(fr.request_state, saved_request.request_state)

        # Tear down
        saved_request.delete()


if __name__ == '__main__':
    unittest.main()
