from django.http import HttpResponse
import urllib.request
import json

# Create your views here.
def getAll(request):
	response = urllib.request.urlopen("http://localhost:59253/api/search").read()
	# jsondata = json.loads(response.decode('utf-8'))
	return HttpResponse(response, content_type='application/json')