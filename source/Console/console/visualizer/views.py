from django.shortcuts import render

# Create your views here.

def showviz(request):
	return render(request, 'visualizer/main.html')