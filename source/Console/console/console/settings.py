"""
Django settings for console project.

Generated by 'django-admin startproject' using Django 2.0.7.

For more information on this file, see
https://docs.djangoproject.com/en/2.0/topics/settings/

For the full list of settings and their values, see
https://docs.djangoproject.com/en/2.0/ref/settings/
"""

import os, json

# Build paths inside the project like this: os.path.join(BASE_DIR, ...)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

with open(BASE_DIR + '\\config.json', "r") as config_file:
    config = json.load(config_file)

# Quick-start development settings - unsuitable for production
# See https://docs.djangoproject.com/en/2.0/howto/deployment/checklist/

# SECURITY WARNING: keep the secret key used in production secret!
SECRET_KEY = config['secret_key']

# SECURITY WARNING: don't run with debug turned on in production!
DEBUG = config['debug']

ALLOWED_HOSTS = []


# Application definition

INSTALLED_APPS = [
    'django.contrib.admin',
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'django.contrib.staticfiles',
    'visualizer',
]

MIDDLEWARE = [
    'django.middleware.security.SecurityMiddleware',
    'django.contrib.sessions.middleware.SessionMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
]

ROOT_URLCONF = 'console.urls'

TEMPLATES = [
    {
        'BACKEND': 'django.template.backends.django.DjangoTemplates',
        'DIRS': [],
        'APP_DIRS': True,
        'OPTIONS': {
            'context_processors': [
                'django.template.context_processors.debug',
                'django.template.context_processors.request',
                'django.contrib.auth.context_processors.auth',
                'django.contrib.messages.context_processors.messages',
            ],
        },
    },
]

WSGI_APPLICATION = 'console.wsgi.application'


# Database
# https://docs.djangoproject.com/en/2.0/ref/settings/#databases

DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.sqlite3',
        'NAME': os.path.join(BASE_DIR, 'db.sqlite3'),
    }
}


# Password validation
# https://docs.djangoproject.com/en/2.0/ref/settings/#auth-password-validators

AUTH_PASSWORD_VALIDATORS = [
    {
        'NAME': 'django.contrib.auth.password_validation.UserAttributeSimilarityValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.MinimumLengthValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.CommonPasswordValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.NumericPasswordValidator',
    },
]


# Internationalization
# https://docs.djangoproject.com/en/2.0/topics/i18n/

LANGUAGE_CODE = 'en-us'
TIME_ZONE = 'UTC'
USE_I18N = True
USE_L10N = True
USE_TZ = True


# Static files (CSS, JavaScript, Images)
# https://docs.djangoproject.com/en/2.0/howto/static-files/

STATIC_URL = '/static/'


if config.get('Ldap') is not None:
    print('Setting up LDAP configuration')
    import ldap
    from django_auth_ldap.config import LDAPSearch, NestedActiveDirectoryGroupType,MemberDNGroupType

    ldapconfig = config['Ldap']

    if ldapconfig['tls'] == True: 
        AUTH_LDAP_START_TLS = True

    AUTH_LDAP_BIND_DN = ldapconfig['bind_user_dn']
    AUTH_LDAP_BIND_PASSWORD = ldapconfig['bind_user_pw']
    AUTH_LDAP_USER_SEARCH = LDAPSearch(ldapconfig['users_ou'],ldap.SCOPE_SUBTREE, "(samaccountname=%(user)s)")

    AUTH_LDAP_SERVER_URI = "LDAP://" + ldapconfig['server']


    # Set up the basic group parameters.
    AUTH_LDAP_GROUP_SEARCH = LDAPSearch(ldapconfig['domain_dn'],
        ldap.SCOPE_SUBTREE,
        '(objectClass=groupOfNames)',
    )

    AUTH_LDAP_GROUP_TYPE = MemberDNGroupType('member')

    # Simple group restrictions
    AUTH_LDAP_REQUIRE_GROUP = ldapconfig['access_group_dn']
    # AUTH_LDAP_DENY_GROUP = 'cn=disabled,ou=django,ou=groups,dc=example,dc=com'

    # Populate the Django user from the LDAP directory.
    AUTH_LDAP_USER_ATTR_MAP = {
        'first_name': 'givenName',
        'last_name': 'sn',
        'email': 'mail',
    }

    AUTH_LDAP_USER_FLAGS_BY_GROUP = {
        'is_active': ldapconfig['active'],
        'is_superuser': ldapconfig['superuser'],
        'is_staff': ldapconfig['staff'],
    }

    AUTH_LDAP_ALWAYS_UPDATE_USER = True

    # Use LDAP group membership to calculate group permissions.
    # AUTH_LDAP_FIND_GROUP_PERMS = True

    # Cache distinguised names and group memberships for an hour to minimize
    # LDAP traffic.
    AUTH_LDAP_CACHE_TIMEOUT = ldapconfig['cachetimeout']

    ############################## django-auth-ldap ##############################
    if DEBUG:
        import logging, logging.handlers
        ldap_logfile = config['logfile']
        ldap_logger = logging.getLogger('django_auth_ldap')
        ldap_logger.setLevel(logging.DEBUG)

        ldap_handler = logging.handlers.RotatingFileHandler(
           ldap_logfile, maxBytes=1024 * 500, backupCount=5)

        ldap_logger.addHandler(ldap_handler)
    ############################ end django-auth-ldap ############################

    # Keep ModelBackend around for per-user permissions and maybe a local
    # superuser.
    AUTHENTICATION_BACKENDS = (
        'django_auth_ldap.backend.LDAPBackend',
        'django.contrib.auth.backends.ModelBackend',
    )