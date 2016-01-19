using System.Collections.Generic;

namespace YouTrackSharp.Import
{
	/*
	 {"items":[{
	 * "ItemReport":{
	 *	"imported":false,
	 *	"errors":[{
	 *		"error":{
	 *			"type":"REQUIRED_FIELD",
	 *			"fieldName":"author",
	 *			"value":"Field is required",
	 *			"format":null
	 *		}
	 *	}],
	 *	"empty":false,
	 *	"id":null
	 *	}}],
	 *	"failed":true,
	 *	"generalError":null,
	 *	"notEmpty":true}
	 *	
	 * {"items":[{"ItemReport":{"imported":false,"errors":[{"error":{"type":"UNKNOWN_VALUE","fieldName":"author.login","value":"Value is unknown","format":null}}],"empty":false,"id":null}}],"failed":true,"generalError":null,"notEmpty":true}
	*/
	public class ImportResponse
	{
		public List<ImportedItemReport> Items { get; set; }
		public bool Failed { get; set; }
		public string GeneralError { get; set; }
		public bool NotEmpty { get; set; }
	}
}