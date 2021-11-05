import dataclasses
from typing import Dict


@dataclasses.dataclass(frozen=True)
class Contract():
    name: str
    has_return: bool
    can_run_async: bool = False
    default_async: bool = False


REMOTE_METHODS: Dict[str, Contract] = {
    'create_building': Contract('CreateBuilding', True, True),
    'create_node': Contract('CreateNode', True, True),
    'create_prop': Contract('CreateProp', True, True),
    'create_segment': Contract('CreateSegment', True, True),
    'create_segments': Contract('CreateSegments', True, True),
    'create_tree': Contract('CreateTree', True, True),
    'delete_object': Contract('DeleteObject', True, True),
    'exists_prefab': Contract('ExistsPrefab', True),
    'get_natural_resource_cell_single': Contract(
        'GetNaturalResourceCellSingle', True
    ),
    'get_natural_resource_cells': Contract(
        'GetNaturalResourceCells', True
    ),
    'get_object_from_id': Contract('GetObjectFromId', True),
    'get_object_starting_from_index': Contract(
        'GetObjectsStartingFromIndex', True
    ),
    'get_segment_for_node_id': Contract('GetSegmentsForNodeId', True),
    'get_terrain_height': Contract('GetTerrainHeight', True),
    'get_water_level': Contract('GetWaterLevel', True),
    'move_object': Contract('MoveObject', True, True),
    'remove_render_object': Contract('RemoveRenderObject', False),
    'render_circle': Contract('RenderCircle', True, True),
    'render_vector': Contract('RenderVector', True, True),
    'set_natural_resource': Contract('SetNaturalResource', True, True),
}
