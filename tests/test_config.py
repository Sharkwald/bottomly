import unittest
from config import ConfigKeys, Config


class TestConfig(unittest.TestCase):
    def test_all_defined_config_keys_configured(self):
        config = Config()
        config_values = [config.get_config_value(k) for k in ConfigKeys]

        for val in config_values:
            self.assertIsNotNone(val)


if __name__ == '__main__':
    unittest.main()
