#!/usr/bin/env bash

if [ ! ${APPCENTER_SOURCE_DIRECTORY} ]
then
    echo "Скрипт запущен локально"
    if [ ! $1 ] && [ ! $2 ] && [ ! $3 ] && [ ! $4 ] && [ ! $5 ] && [ ! $6 ]
        then 
            echo -e "Нет аргумента для задания имени пакета, отмена.\nИспользование: sh <скрипт> <имя пакета> <имя приложения> <версия> <база> <приминение цвета(true/false)> <RGB код цвета> <appcenterkey>"
            exit 1
    fi

    PACKAGENAME=$1
    LABEL=$2
    VERSION=$3
    BASE=$4
    DECLARE_CUSTOM_COLOR=$5
    CUSTOM_COLOR=$6    
    APPCENTER_KEY=$7
	
	INFO_PLIST_FILE=xamarinJKH.iOS/info.plist
    CLIENT_SCRIPT=xamarinJKH/Server/RestClientMP.cs
    ORIGINAL_GS=xamarinJKH.iOS/GoogleService-Info.plist
    GS_PATH=xamarinJKH.iOS/GoogleService-Info_${PACKAGENAME}.plist
    ORIGINAL_ICON=xamarinJKH.iOS/Resources/icon_login.png 
    ICON_PATH=xamarinJKH.iOS/Resources/icon_login_${PACKAGENAME}.png 
    MAINPAGE=xamarinJKH/MainPage.xaml.cs
	
	APPSET=xamarinJKH.iOS/AppIcon.appiconset_${PACKAGENAME}
	APPSETCURR=xamarinJKH.iOS/Assets.xcassets/AppIcon.appiconset
	PROJFILE=xamarinJKH.iOS/xamarinJKH.iOS.csproj
	APP=../xamarinJKH/App.xaml.cs
	
else
    echo "##[section][Pre-Build] Setting up Environment variables"
    echo "##[section][Pre-Build] Getting data from variable PACKAGE_NAME"
    PACKAGENAME=${PACKAGE_NAME}
    if [ ! ${PACKAGENAME} ]
    then
        echo "##[section][Pre-Build] No package name provided. Aborting"
        exit 1
    fi
    echo "##[section][Pre-Build] Getting data from variable LABEL_NAME"
    LABEL=${LABEL_NAME}
    if [ ! ${LABEL} ]
    then
        echo "##[section][Pre-Build] No label provided. Aborting"
        exit 1
    fi

    echo "##[section][Pre-Build] Getting data from variable VERSION_BUILD"
    VERSION=${VERSION_BUILD}
    if [ ! ${VERSION} ] 
	then
        echo "##[section][Pre-Build] No version provided. Aborting"
        exit 1
    fi

    echo "##[section][Pre-Build] Getting data from variable DATABASE"
    BASE=${DATABASE}
    if [ ! ${BASE} ]
	then
        echo "##[section][Pre-Build] No database name provided. Aborting"
        exit 1
    fi
	
	echo "##[section][Pre-Build] Getting data from variable APPCENTER_KEY"
    APPCENTER_KEY=${APPCENTERKEY}
    if [ ! ${APPCENTER_KEY} ]; then
        echo "##[section][Pre-Build] No appcenter key provided. Aborting"
        exit 1
    fi

    ROOT=${APPCENTER_SOURCE_DIRECTORY}/xamarinJKH.iOS
    CLIENT_SCRIPT=${APPCENTER_SOURCE_DIRECTORY}/xamarinJKH/Server/RestClientMP.cs
	INFO_PLIST_FILE=${ROOT}/Info.plist
    ORIGINAL_ICON=${ROOT}/Resources/icon_login.png
    ICON_PATH=${ROOT}/Resources/icon_login_${PACKAGENAME}.png
    ORIGINAL_GS=${ROOT}/GoogleService-Info.plist
    GS_PATH=${ROOT}/GoogleService-Info_${PACKAGENAME}.plist
    DECLARE_CUSTOM_COLOR=${DECLARECOLOR}
    CUSTOM_COLOR=${CUSTOMCOLOR}
    MAINPAGE=${APPCENTER_SOURCE_DIRECTORY}/xamarinJKH/MainPage.xaml.cs
    CUSTOM_NAME=${CUSTOMNAME}
	APPSET=${ROOT}/AppIcon.appiconset_${PACKAGENAME}
	APPSETCURR=${ROOT}/Assets.xcassets/AppIcon.appiconset
	PROJFILE=${ROOT}/xamarinJKH.iOS.csproj
	APP=${APPCENTER_SOURCE_DIRECTORY}/xamarinJKH/App.xaml.cs
	
fi

if [ ${#APPCENTER_KEY} -gt 0 ]
    then
        echo "##[section][Pre-Build] Setting appcenter key"
        sed -i.bak "s/AppCenter.Start/AppCenter.Start(\"${APPCENTER_KEY}\",typeof(Analytics), typeof(Crashes));\/\//" ${APP}
        rm -f ${APP}.bak
        echo 
        echo
fi

cat ${APP} | grep "AppCenter.Start([A-Za-z|\"|=|-|;|,|0-9|\-|\n|+|' '|(|)]*);"

if [ ${DECLARE_CUSTOM_COLOR} == 1 ]
 then
    if [ ${CUSTOM_COLOR} ]
	then
        echo "##[section][Pre-Build] Setting custom color for an interface"
        sed -i.bak "s/var color = !string.IsNullOrEmpty(Settings.MobileSettings.color) ? $\"#{Settings.MobileSettings.color}\" :\"#FF0000\";/var color = \"#${CUSTOM_COLOR}\";Settings.MobileSettings.color = \"${CUSTOM_COLOR}\";/" ${MAINPAGE}
        echo "##[section][Pre-Build] Custom color is set to #${CUSTOM_COLOR}"
        cat ${MAINPAGE} | grep "var color = \"#[0-9|A-Za-z]*\";"
    fi

    if [ ${CUSTOM_NAME} ]
	then
        echo "##[section][Pre-Build] Setting custom name of a company"
        sed -i.bak "s/UkName.Text = Settings.MobileSettings.main_name;/UkName.Text = \"${CUSTOM_NAME}\";Settings.MobileSettings.main_name = \"${CUSTOM_NAME}\";/" ${MAINPAGE}
        echo "##[section][Pre-Build] Custom name is set to ${CUSTOM_NAME}"
    fi
fi


  if [  -a ${ICON_PATH} ]
    then
        cat ${ICON_PATH} > ${ORIGINAL_ICON}
        echo "##[section][pre-build] icon data is copied to original icon"
    else
        echo error: "##[section][pre-build] icon file is not found, this change will not apply. aborting"
        exit 1
    fi
	
	if [ -a ${GS_PATH} ]
    then
        cat ${GS_PATH} > ${ORIGINAL_GS}
        echo "##[section][pre-build] google services file has been changed to one for ${packagename}"
    else
        echo error: "##[section][pre-build] google services for ${packagename} not found. aborting"
        exit 1
    fi

if [ -e ${APPSET} ]
 then  
  cp -a ${APPSET}/*.* ${APPSETCURR}  
  echo "##[section][pre-build] APPSET files has been changed to one for ${APPSET}"
 else
  echo error: "##[section][pre-build] APPSET for ${APPSET} not found. aborting"
  exit 1
fi

if [ -e "$INFO_PLIST_FILE" ]
then
    echo "Updating package name to $PACKAGE_NAME in Info.plist"
    plutil -replace CFBundleIdentifier -string $PACKAGE_NAME $INFO_PLIST_FILE
    echo "Updating package name to $PACKAGE_NAME in Info.plist"
	
	echo "Replace spaces for symbols in $LABEL"
	LABEL="${LABEL// /" "}"	
	
	echo "Updating display name to $LABEL in Info.plist"
    plutil -replace CFBundleDisplayName -string "${LABEL}" ${INFO_PLIST_FILE}
	# /usr/libexec/PlistBuddy -c "Set :CFBundleDisplayName  NewBundleDisplayName" $INFO_PLIST_FILE
    echo "Updating display name to $LABEL in Info.plist"
	
	
	
	echo "Updating version to $VERSION in Info.plist"
    plutil -replace CFBundleVersion -string $VERSION $INFO_PLIST_FILE
    echo "Updating version to $VERSION in Info.plist"
	
	echo "Updating version short string to $VERSION in Info.plist"
    plutil -replace CFBundleShortVersionString -string $VERSION $INFO_PLIST_FILE
    echo "Updating version short string  to $VERSION in Info.plist"	

    echo "File content:"
    cat $INFO_PLIST_FILE
fi


if [ ${#CLIENT_SCRIPT} -gt 0 ]; then
    if [ ${BASE} ]; then
        echo "##[section][Pre-Build] Changing database name in ${CLIENT_SCRIPT} to ${BASE}"
        sed -i.bak -e "s/public const string SERVER_ADDR = \"https:\/\/api.sm-center.ru\/[a-z|A-Z|0-9|\.|\/|\:|\-|\_|]*\";/public const string SERVER_ADDR = \"https:\/\/api.sm-center.ru\/${BASE}\";/" $CLIENT_SCRIPT
        rm -f ${CLIENT_SCRIPT}.bak
    else
        echo ERROR: "##[section][Pre-Build] No base variable set. Aborting"
        exit 1
    fi
else
    echo ERROR: "##[section][Pre-Build] No RestClientMP.cs found. Aborting"
    exit 1
fi

if [ ${#PROJFILE} -gt 0 ]; then
    if [ ${BASE} ]; then
        echo "##[section][Pre-Build] Changing ipa name in ${PROJFILE} to ${PACKAGE_NAME}"
        sed -i.bak -e "s/<IpaPackageName>[a-z|A-Z|0-9|\.|\/|\:|\-|\_|]<\/IpaPackageName>\";/<IpaPackageName>${PACKAGE_NAME}<\/IpaPackageName>\";/" $PROJFILE
        rm -f ${PROJFILE}.bak
    else
        echo ERROR: "##[section][Pre-Build] No PACKAGE_NAME variable set. Aborting"
        exit 1
    fi
else
    echo ERROR: "##[section][Pre-Build] No xamarinJKH.iOS.csproj found. Aborting"
    exit 1
fi

echo
echo DONE
