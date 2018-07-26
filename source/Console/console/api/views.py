from django.http import HttpResponse, QueryDict
from django.contrib.auth.decorators import login_required
from urllib.request import urlopen, Request
import json

# Create your views here.

@login_required
def passthrough(request):
	proxy="http://localhost:59253"
	if request.method == 'GET':
		url = proxy + request.path + "?" + request.GET.urlencode()
		response = urlopen(url)
		return HttpResponse(response.read(), content_type='application/json')

	elif request.method == 'POST':
		url = proxy + request.path
		body = request.body
		req = Request(url,data=body, headers={'Content-Type': 'application/json'}, method='POST')
		response = urlopen(req).read()
		return HttpResponse(response, content_type='application/json')