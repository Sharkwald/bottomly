from pymodm import MongoModel, fields
from pymongo import WriteConcern
from config import Config


class Cocktail(MongoModel):
    id = fields.CharField(primary_key=True)
    name = fields.CharField()
    description = fields.CharField()
    ingredients = fields.ListField()
    instructions = fields.ListField()
    source = fields.CharField()
    url = fields.URLField()
    image = fields.URLField()

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection
