# coding=utf-8
import threading
import requests
import time

from flask import Flask

from slack_channel.slack_event_handler import SlackEventHandler

app = Flask(__name__)

@app.route("/", methods=["GET", "POST"])
def start_up():
    print("Flask has been accessed.")
    return "Hello, I am bottomly."

@app.before_first_request
def activate_job():
    def spin_up_slack_socket():
        handler = SlackEventHandler(app.debug)
        handler.handle_slack_context()

    thread = threading.Thread(target=spin_up_slack_socket)
    thread.start()




def start_runner():
    def start_loop():
        not_started = True
        while not_started:
            print('In start loop')
            try:
                r = requests.get("http://localhost:5000")
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