from __future__ import annotations

import abc
import functools
from typing import Any, Dict, List, Optional, Type, overload

from typing_extensions import Literal

from ... import meta, protocol
from .. import header, in_, objects, out_, utils
from . import abc as game_abc
from . import cache, iterobj

_CREATE_OBJ_MSG: Dict[str, Type[header.BaseMessage]] = {
    'prop': out_.CreatePropMessage,
    'building': out_.CreateBuildingMessage,
    'node': out_.CreateNodeMessage,
    'tree': out_.CreateTreeMessage,
    'segment': out_.CreateSegmentMessage
}

class Game(game_abc.BaseGame, metaclass=meta.Singleton):
    def __init__(self) -> None:
        self._async_mode = False
        self._cache = cache.CacheManager()

    @property
    def async_mode(self) -> bool:
        return self._async_mode

    @async_mode.setter
    def async_mode(self, value: bool) -> None:
        self._async_mode = value

    @abc.abstractmethod
    def _remote_call(self, contract: protocol.Contract, message: Any) -> Any:
        ...

    @property
    def trees(self) -> iterobj.GameObjectIterator[objects.Tree, in_.TreeData]:
        return iterobj.GameObjectIterator(self,
            objects.Tree, in_.TreeData,
            functools.partial(self._get_batch_object, type_='tree')
        )

    @property
    def buildings(self) -> iterobj.GameObjectIterator[objects.Building, in_.BuildingData]:
        return iterobj.GameObjectIterator(self,
            objects.Building, in_.BuildingData,
            functools.partial(self._get_batch_object, type_='building')
        )

    @property
    def nodes(self) -> iterobj.GameObjectIterator[objects.Node, in_.NetNodeData]:
        return iterobj.GameObjectIterator(self,
            objects.Node, in_.NetNodeData,
            functools.partial(self._get_batch_object, type_='node')
        )

    @property
    def props(self) -> iterobj.GameObjectIterator[objects.Prop, in_.PropData]:
        return iterobj.GameObjectIterator(self,
            objects.Prop, in_.PropData,
            functools.partial(self._get_batch_object, type_='prop')
        )

    @property
    def segments(self) -> iterobj.GameObjectIterator[objects.Segment, in_.NetSegmentData]:
        return iterobj.GameObjectIterator(self,
            objects.Segment, in_.NetSegmentData,
            functools.partial(self._get_batch_object, type_='segment')
        )

    # object handling

    def _get_cache(self, type_: str, id_: int = 0, idString: Optional[str] = None):
        return self._cache[cache.CacheKey(type_, id_, idString)]

    def _get_obj(self, type_: str, id_: int = 0, idString: Optional[str] = None):
        ret = self._remote_call(
            protocol.REMOTE_METHODS['get_object_from_id'],
            out_.GetObjectMessage(id=id_, type=type_, idString=idString)
        )
        assert isinstance(ret, header.BaseMessage)
        return ret

    def _move_obj(self, id_: int, type_: str, position: utils.IPositionable,
        angle: Optional[float] = None
    ):
        return self._remote_call(
            protocol.REMOTE_METHODS['move_object'],
            out_.MoveMessage(
                id=id_, type=type_, position=position,
                angle=0 if angle is None else angle,
                is_angle_defined=angle is not None
            )
        )

    def _delete_obj(self, id_: int, type_: str, keep_nodes: bool = False):
        return self._remote_call(
            protocol.REMOTE_METHODS['delete_object'],
            out_.DeleteObjectMessage(id=id_, type=type_, keep_nodes=keep_nodes)
        )

    def _create_obj(self, obj_type: str, **kwargs):
        return self._remote_call(
            protocol.REMOTE_METHODS[f'create_{obj_type}'],
            _CREATE_OBJ_MSG[obj_type](**kwargs)
        )

    # Resource handling

    def _get_resource(self, id_: int):
        return self._remote_call(
            protocol.REMOTE_METHODS['get_natural_resource_cell_single'], id_
        )

    def _set_resource(self, id_: int, type: str, value: int):
        return self._remote_call(
            protocol.REMOTE_METHODS['set_natural_resource'],
            out_.SetNaturalResourceMessage(
                cell_id=id_, type=type, value=value
            )
        )

    # Segment attribute

    def _get_adj_segments(self, id_: int) -> List[objects.Segment]:
        msg = self._remote_call(
            protocol.REMOTE_METHODS['get_segment_for_node_id'], id_
        )
        assert isinstance(msg, list)
        ret: List[objects.Segment] = []
        for _ in msg:
            assert isinstance(_, header.BaseMessage)
            ret.append(objects.Segment.from_message(self, _))
        return ret

    # Iterator support

    def _get_batch_object(self, id_: int, type_: str):
        ret = self._remote_call(
            protocol.REMOTE_METHODS['get_object_starting_from_index'],
            out_.GetObjectsFromIndexMessage(index=id_, type=type_)
        )
        assert isinstance(ret, in_.BatchObjectMessage)
        return ret['array']

    # API - get object

    def get_prop(self, id_: int, use_cache: bool = True) -> objects.Prop:
        pattern = {'id_': id_, 'type_': 'prop'}
        ret = self._get_cache(**pattern)
        if not (isinstance(ret, objects.Prop) and use_cache):
            ret = objects.Prop.from_message(
                self, self._get_obj(**pattern)
            )
        return ret

    def get_tree(self, id_: int, use_cache: bool = True) -> objects.Tree:
        pattern = {'id_': id_, 'type_': 'tree'}
        ret = self._get_cache(**pattern)
        if not (isinstance(ret, objects.Tree) and use_cache):
            ret = objects.Tree.from_message(
                self, self._get_obj(**pattern)
            )
        return ret

    def get_building(self, id_: int, use_cache: bool = True) -> objects.Building:
        pattern = {'id_': id_, 'type_': 'building'}
        ret = self._get_cache(**pattern)
        if not (isinstance(ret, objects.Building) and use_cache):
            ret = objects.Building.from_message(
                self, self._get_obj(**pattern)
            )
        return ret

    def get_node(self, id_: int, use_cache: bool = True) -> objects.Node:
        pattern = {'id_': id_, 'type_': 'node'}
        ret = self._get_cache(**pattern)
        if not (isinstance(ret, objects.Node) and use_cache):
            ret = objects.Node.from_message(
                self, self._get_obj(**pattern)
            )
        return ret

    def get_segment(self, id_: int, use_cache: bool = True) -> objects.Segment:
        pattern = {'id_': id_, 'type_': 'segment'}
        ret = self._get_cache(**pattern)
        if not (isinstance(ret, objects.Segment) and use_cache):
            ret = objects.Segment.from_message(
                self, self._get_obj(**pattern)
            )
        return ret

    # API - create object

    def create_prop(self,
        position: utils.IPositionable, prefab_name: str, angle: float
    ):
        ret = self._create_obj('prop',
            Position=position.position, Type=prefab_name, Angle=angle
        )
        if isinstance(ret, header.BaseMessage):
            return objects.Prop.from_message(self, ret)
        return None

    def create_tree(self, position: utils.IPositionable, prefab_name: str):
        ret = self._create_obj('tree',
            Position=position.position, prefab_name=prefab_name
        )
        if isinstance(ret, header.BaseMessage):
            return objects.Tree.from_message(self, ret)
        return None

    def create_building(self,
        position: utils.IPositionable, type_: str, angle: float = 0
    ) -> Optional[objects.Building]:
        ret = self._create_obj('building',
            Position=position.position, Type=type_, Angle=angle
        )
        if isinstance(ret, header.BaseMessage):
            return objects.Building.from_message(self, ret)
        return None

    def create_node(self,
        position: utils.IPositionable, prefab: str | objects.NetPrefab
    ) -> Optional[objects.Node]:
        if not isinstance(prefab, (str, objects.NetPrefab)):
            raise TypeError("Prefab must be string or NetPrefab")
        ret = self._create_obj('node',
                               Position=position.position,
                               Type=prefab if isinstance(
                                   prefab, str) else prefab.name
                               )
        if isinstance(ret, header.BaseMessage):
            return objects.Node.from_message(self, ret)
        return None

    @overload
    def _create_segment(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None,
        middle_pos: Optional[utils.IPositionable] = None,
        autosplit: Literal[False] = False
    ) -> Optional[objects.Segment]:
        ...

    @overload
    def _create_segment(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None,
        middle_pos: Optional[utils.IPositionable] = None,
        autosplit: Literal[True] = True
    ) -> Optional[List[objects.Segment]]:
        ...

    def _create_segment(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None,
        middle_pos: Optional[utils.IPositionable] = None,
        autosplit: bool = False
    ):
        start_is_node = isinstance(start_node, objects.Node)
        end_is_node = isinstance(end_node, objects.Node)
        start_node_id = start_node.id_ \
            if isinstance(start_node, objects.Node) else 0
        end_node_id = end_node.id_ \
            if isinstance(end_node, objects.Node) else 0
        kwargs = {
            'start_node_id': start_node_id,
            'end_node_id': end_node_id,
            'start_position': None if not start_is_node else start_node.position,
            'end_position': None if not end_is_node else end_node.position,
            'net_options': utils.NetOptions.ensure(type_),
            'start_dir': start_dir,
            'end_dir': end_dir,
            'control_point': None if middle_pos is None else middle_pos.position,
            'auto_split': autosplit
        }

        ret = self._create_obj('segment', **kwargs)
        if isinstance(ret, header.BaseMessage):
            return objects.Segment.from_message(self, ret)
        if isinstance(ret, list):
            return [objects.Segment.from_message(self, _) for _ in ret]
        return None
    
    def create_segment(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        control_point: Optional[utils.IPositionable] = None,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None
    ) -> Optional[objects.Segment]:
        if (start_dir is None) ^ (end_dir is None):
            raise ValueError("start_dir and end_dir must be both set or both unset")
        if (control_point is None) == (start_dir is None):
            raise ValueError("control_point and start_dir can't be both present")
        return self._create_segment(
            start_node, end_node, type_,
            middle_pos=control_point, start_dir=start_dir, end_dir=end_dir
        )

    def create_segments(self,
        start_node: utils.IPositionable, end_node: utils.IPositionable,
        type_: Any, *,
        control_point: Optional[utils.IPositionable] = None,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None
    ) -> Optional[List[objects.Segment]]:
        if (start_dir is None) ^ (end_dir is None):
            raise ValueError("start_dir and end_dir must be both set or both unset")
        if (control_point is None) == (start_dir is None):
            raise ValueError("control_point and start_dir can't be both present")
        return self._create_segment(
            start_node, end_node, type_,
            middle_pos=control_point,
            start_dir=start_dir, end_dir=end_dir, autosplit=True
        )

    # Path builder

    def begin_path(
        self, start_node: utils.IPositionable, options: Any
    ) -> objects.PathBuilder:
        return objects.PathBuilder(self, start_node, options)

    # Rendered object handling

    def draw_vector(self,
        vector: utils.IPositionable, origin: utils.IPositionable,
        color: str = 'red', length: float = 20, size: float = 0.1
    ):
        return objects.RenderableObjectHandle(self, id=self._remote_call(
            protocol.REMOTE_METHODS['render_vector'],
            out_.RenderVectorMessage(
                vector=vector.position,
                origin=origin.position,
                color=color,
                length=length,
                size=size
            )
        ))

    def draw_circle(self,
        position: utils.IPositionable, radius: float = 5, color: str = 'red'
    ):
        return objects.RenderableObjectHandle(self, id=self._remote_call(
            protocol.REMOTE_METHODS['render_circle'],
            out_.RenderCircleMessage(
                position=position.position,
                radius=float(radius),
                color=color
            )
        ))

    def remove_render_object(self, id_: int):
        return self._remote_call(
            protocol.REMOTE_METHODS['remove_render_object'], id_
        )

    def clear(self):
        return self.remove_render_object(0)

    # Net prefab handling

    def get_net_prefab(self, name: str, use_cache: bool = True) -> objects.NetPrefab:
        pattern = {'idString': name, 'type_': 'node'}
        ret = self._get_cache(**pattern)
        if not (isinstance(ret, objects.NetPrefab) and use_cache):
            ret = objects.NetPrefab.from_message(
                self, self._get_obj(**pattern)
            )
        return ret

    def is_prefab(self, name: str) -> bool:
        return self._remote_call(
            protocol.REMOTE_METHODS['exists_prefab'],
            name
        )

    def terrain_height(self, pos: utils.IPositionable) -> float:
        return self._remote_call(
            protocol.REMOTE_METHODS['get_terrain_height'],
            pos.position
        )

    def surface_level(self, pos: utils.IPositionable) -> float:
        return self._remote_call(
            protocol.REMOTE_METHODS['get_water_level'],
            pos.position
        )
