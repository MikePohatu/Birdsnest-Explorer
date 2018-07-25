from django.http import HttpResponse
from django.contrib.auth.decorators import login_required
from urllib.request import urlopen
from urllib.parse import urlparse
# Create your views here.

@login_required
def passthrough(request):
	proxy="http://localhost:59253"
	if request.method == 'GET':
		url = proxy + request.path + "?" + request.GET.urlencode()
		print(url)
		response = urlopen(url).read()
		return HttpResponse(response, content_type='application/json')

	elif request.method == 'POST':
		url = proxy + request.path + "?" + request.POST.urlencode()
		print(url)
		response = urlopen(url, data=request.body)
		return HttpResponse(response, content_type='application/json')