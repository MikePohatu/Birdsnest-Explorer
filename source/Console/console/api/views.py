from django.http import HttpResponse
from django.contrib.auth.decorators import login_required
import urllib.request

# Create your views here.

@login_required
def getAll(request):
	response = urllib.request.urlopen("http://localhost:59253/api/search").read()
	return HttpResponse(response, content_type='application/json')