from django.shortcuts import render, redirect
from django.contrib.auth.decorators import login_required
# Create your views here.

@login_required
def showviz(request):
	if request.user.is_authenticated:
		return render(request, 'visualizer/main.html')
	else:
		return redirect(request, 'portal_login')
