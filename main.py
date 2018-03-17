# coding=utf-8
import threading
import requests
import time

from slacksocket import SlackSocket
from commands.google_search import GoogleSearchCommand
from config import Config, ConfigKeys
from flask import Flask, request, make_response, render_template

config = Config()
token = config.get_config_value(ConfigKeys.slack_bot_token)

app = Flask(__name__)

@app.route("/", methods=["GET", "POST"])
def start_up():
    print("Flask has been accessed.")
    return "Hello, I am bottomly."

@app.before_first_request
def activate_job():
    def spin_up_slack_socket():
        print("opening web socket to slack...")
        with SlackSocket(token) as s:
            try:
                for event in s.events():
                    print(event.json)
                    if (is_google_command(event.event)):
                        q = event.event['text'][3:]
                        c = GoogleSearchCommand()
                        result = c.execute(q)
                        response_message = result['title'] + " " + result["link"]
                        send_google_response(response_message, event.event)
            except Exception as ex:
                print("Error! " + str(ex))

    thread = threading.Thread(target=spin_up_slack_socket)
    thread.start()


def is_google_command(slack_event):
    if slack_event['type'] != "message":
        return False
    if not 'text' in slack_event:
        return False
    text = slack_event['text']
    return text.startswith("_g ")

def send_google_response(response_message, slack_event):
    with SlackSocket(token) as s:
        msg = s.send_msg(response_message, slack_event['channel'])
        print(msg.sent)

def start_runner():
    def start_loop():
        not_started = True
        while not_started:
            print('In start loop')
            try:
                r = requests.get('http://localhost/')
                if r.status_code == 200:
                    print('Server started, quiting start_loop')
                    not_started = False
                print(r.status_code)
            except:
                print('Server not yet started')
            time.sleep(2)

    print('Started runner')
    thread = threading.Thread(target=start_loop)
    thread.start()

if __name__ == '__main__':
    start_runner()
    app.run(debug=True)