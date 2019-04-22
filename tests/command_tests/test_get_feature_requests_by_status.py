import unittest

from unittest.mock import patch

from commands import GetFeatureRequestsByStatusCommand
from model.feature_request import FeatureRequest, FeatureRequestState


class TestGetFeatureRequestsByStatus(unittest.TestCase):

    def test_get_requested(self):
        expected_requests = list([FeatureRequest(), FeatureRequest()])
        with patch.object(FeatureRequest, "get_requested",
                          return_value=expected_requests) as test_method:
            c = GetFeatureRequestsByStatusCommand()
            actual_requests = c.execute(FeatureRequestState.REQUESTED)

            self.assertEqual(expected_requests, actual_requests)
            test_method.assert_called_once()

    def test_get_in_progress(self):
        expected_requests = list([FeatureRequest(), FeatureRequest()])
        with patch.object(FeatureRequest, "get_in_progress",
                          return_value=expected_requests) as test_method:
            c = GetFeatureRequestsByStatusCommand()
            actual_requests = c.execute(FeatureRequestState.IN_PROGRESS)

            self.assertEqual(expected_requests, actual_requests)
            test_method.assert_called_once()

    def test_get_delivered(self):
        expected_requests = list([FeatureRequest(), FeatureRequest()])
        with patch.object(FeatureRequest, "get_delivered",
                          return_value=expected_requests) as test_method:
            c = GetFeatureRequestsByStatusCommand()
            actual_requests = c.execute(FeatureRequestState.DELIVERED)

            self.assertEqual(expected_requests, actual_requests)
            test_method.assert_called_once()

    def test_get_rejected(self):
        expected_requests = list([FeatureRequest(), FeatureRequest()])
        with patch.object(FeatureRequest, "get_rejected",
                          return_value=expected_requests) as test_method:
            c = GetFeatureRequestsByStatusCommand()
            actual_requests = c.execute(FeatureRequestState.REJECTED)

            self.assertEqual(expected_requests, actual_requests)
            test_method.assert_called_once()


if __name__ == '__main__':
    unittest.main()