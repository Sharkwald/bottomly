import unittest
from config import Config
from model.feature_request import FeatureRequest, FeatureRequestState

default_user = "testUser1"
default_feature = "some feature please"
default_state = FeatureRequestState.REQUESTED


def create_request(requester=default_user,
                   request=default_feature,
                   request_state=default_state) -> FeatureRequest:
    fr = FeatureRequest(requester=requester,
                        request=request,
                        request_state=str(request_state))
    return fr


def default_request_list() -> list:
    return list([create_request(),
                 create_request(request_state=FeatureRequestState.IN_PROGRESS),
                 create_request(request_state=FeatureRequestState.DELIVERED),
                 create_request(request_state=FeatureRequestState.REJECTED)])


class TestFeatureRequest(unittest.TestCase):
    def setUp(self) -> None:
        Config().connect_to_db()
        old_requests = FeatureRequest.objects.all()
        for ofr in old_requests:
            ofr.delete()

    def tearDown(self) -> None:
        old_requests = FeatureRequest.objects.all()
        for ofr in old_requests:
            ofr.delete()

    @staticmethod
    def persist_default_requests() -> list:
        requests_to_save = default_request_list()
        saved_requests = list([])
        for fr in requests_to_save:
            saved_requests.append(fr.save())
        return saved_requests

    def test_persistence(self):
        # Arrange
        requester = default_user
        request = "make something happen"
        request_state = FeatureRequestState.IN_PROGRESS
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

    def test_get_requested(self):
        # Arrange
        requests = self.persist_default_requests()
        expected_request = requests[0]

        # Act
        requested = FeatureRequest.get_requested()

        # Assert
        self.assertEqual(len(requested), 1)
        actual = requested[0]
        self.assertEqual(actual, expected_request)

    def test_get_in_progress(self):
        # Arrange
        requests = self.persist_default_requests()
        expected_request = requests[1]

        # Act
        requested = FeatureRequest.get_in_progress()

        # Assert
        self.assertEqual(len(requested), 1)
        actual = requested[0]
        self.assertEqual(actual, expected_request)

    def test_get_delivered(self):
        # Arrange
        requests = self.persist_default_requests()
        expected_request = requests[2]

        # Act
        requested = FeatureRequest.get_delivered()

        # Assert
        self.assertEqual(len(requested), 1)
        actual = requested[0]
        self.assertEqual(actual, expected_request)

    def test_get_rejected(self):
        # Arrange
        requests = self.persist_default_requests()
        expected_request = requests[3]

        # Act
        requested = FeatureRequest.get_rejected()

        # Assert
        self.assertEqual(len(requested), 1)
        actual = requested[0]
        self.assertEqual(actual, expected_request)


if __name__ == '__main__':
    unittest.main()
