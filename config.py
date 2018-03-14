import os
from enum import Enum


class ConfigKeys(Enum):
    google_api_key = "bottomly_google_api_key"
    google_cse_id = "bottomly_google_cse_id"


class Config(object):

    _key_err_messages = {
        ConfigKeys.google_api_key: 'Google API key is not configured',
        ConfigKeys.google_cse_id: 'Google custom search engine ID is not configured'
    }

    def get_config_value(self, key):
        if key.value in os.environ:
            return os.environ.get(key.value)
        else:
            raise EnvironmentError(Config._key_err_messages[key])

    def __init__(self):
        super(Config, self).__init__()
