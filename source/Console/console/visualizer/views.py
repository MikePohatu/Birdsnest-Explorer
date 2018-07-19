from django.shortcuts import render
from django.contrib.auth.decorators import login_required
# Create your views here.

@login_required
def showviz(request):
	return render(request, 'visualizer/main.html')