// version 2.0.2 (last revision Mar, 2024)

window.addEventListener("unhandledrejection", async function (event) 
{
	event.preventDefault();
	try
	{
		await window.netproxyasync("./api/errorlog",
		{
			"event": "-",
			"errormessage": event.reason.message,
			"errorstack": event.reason.stack,
			"path": event.reason.path,
			"source": event.type
		});
	}
	catch (err)
	{
		console.error(err);
	}
});


window.onerror = async function (message, source, lineno, colno, error)
{
	try
	{
		if (source.indexOf("api/errorlog") < 0)
			await window.netproxyasync("./api/errorlog",
			{
				"event": message,
				"errormessage" : error.message,
				"errorstack": error.stack,
				"path": source,
				"source": "line:" + lineno + " col:" + colno
			});
	}
	catch (err)
	{
		console.error(err);
	}
	return true;
}

