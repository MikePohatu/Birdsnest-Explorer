from django.shortcuts import render, redirect

# Create your views here.
def welcome(request):
	return render(request, 'portal/welcome.html')
