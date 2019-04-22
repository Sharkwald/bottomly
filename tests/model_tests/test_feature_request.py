import unittest
from config import Config
from model.feature_request import FeatureRequest, FeatureRequestState


class TestFeatureRequest(unittest.TestCase):
    def setUp(self):
        Config().connect_to_db()
        old_requests = FeatureRequest.objects.all()
        for ofr in old_requests:
            ofr.delete()

    def test_persistence(self):
        # Arrange
        requester = "testUser1"
        request = "make something happen"
        request_state = FeatureRequestState.INPROG
        fr = FeatureRequest(requester=requester,
                            request=request,
                            request_state=str(request_state))

        # Act
        fr.save()

        # Assert
        loaded_request = FeatureRequest.objects.all()[0]
        self.assertIsNotNone(loaded_request._id)
        self.assertEqual(fr.requester, loaded_request.requester)
        self.assertEqual(fr.request, loaded_request.request)
        self.assertEqual(fr.request_state, loaded_request.request_state)


if __name__ == '__main__':
    unittest.main()
